//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using HonkSharp.Laziness;

namespace AngouriMath
{
    partial record Entity
    {
        /// <summary>
        /// This should NOT be called inside itself
        /// </summary>
        protected abstract Entity InnerSimplify();

        /// <summary>
        /// This is the result of naive simplifications. In other 
        /// symbolic algebra systems it is called "Automatic simplification".
        /// It only performs an active operation in the first call,
        /// next time it is free to call it in terms of CPU usage. For
        /// consistency's sake, consider the call of this property
        /// as free as the addressing of a field.
        /// </summary>
        /// <example>
        /// <code>
        /// using System;
        /// using static AngouriMath.MathS;
        /// 
        /// var x = Var("x");
        /// var expr = Sqr(Sin(x + 0)) + Sqr(Cos(x / 1));
        /// Console.WriteLine(expr);
        /// Console.WriteLine(expr.InnerSimplified);
        /// Console.WriteLine(expr.Simplify());
        /// </code>
        /// Prints
        /// <code>
        /// sin(x + 0) ^ 2 + cos(x / 1) ^ 2
        /// sin(x) ^ 2 + cos(x) ^ 2
        /// 1
        /// </code>
        /// </example>
        public Entity InnerSimplified => innerSimplified.GetValue(static @this => @this.InnerSimplifyWithCheck(), this);
        private LazyPropertyA<Entity> innerSimplified;


        private Entity InnerActionWithCheck(IEnumerable<Entity> directChildren, Entity innerSimplifiedOrEvaled, bool returnThisIfNaN)
        {
            if (innerSimplifiedOrEvaled.DirectChildren.Any(c => c == MathS.NaN))
                return MathS.NaN;
            if (DomainsFunctional.FitsDomainOrNonNumeric(innerSimplifiedOrEvaled, Codomain))
                return innerSimplifiedOrEvaled;
            if (returnThisIfNaN)
                return this;
            else
                return MathS.NaN;
        }

        /// <summary>
        /// Make sure you call this function inside of <see cref="InnerSimplify"/>
        /// </summary>
        internal Entity InnerSimplifyWithCheck()
            => InnerActionWithCheck(DirectChildren.Select(c => c.InnerSimplified), InnerSimplify(), true);

        /// <summary>
        /// This should NOT be called inside itself
        /// </summary>
        protected abstract Entity InnerEval();
        
        /// <summary>
        /// Make sure you call this function inside of <see cref="InnerEval"/>
        /// </summary>
        protected Entity InnerEvalWithCheck()
            => InnerActionWithCheck(DirectChildren.Select(c => c.Evaled), InnerEval(), false);


        /// <summary>
        /// Expands an equation trying to eliminate all the parentheses ( e. g. 2 * (x + 3) = 2 * x + 2 * 3 )
        /// </summary>
        /// <param name="level">
        /// The number of iterations (increase this argument in case if some parentheses remain)
        /// </param>
        /// <returns>
        /// An expanded Entity if it wasn't too complicated,
        /// current entity otherwise
        /// To change the limit use <see cref="MathS.Settings.MaxExpansionTermCount"/>
        /// </returns>
        /// <example>
        /// <code>
        /// using System;
        /// using static AngouriMath.MathS;
        /// 
        /// var (x, y) = Var("x", "y");
        /// 
        /// var expr = (x + 3) * (Sin(y) + 5);
        /// Console.WriteLine(expr);
        /// Console.WriteLine(expr.Expand());
        /// Console.WriteLine("-----------------------------------");
        /// var expr2 = Pow(x + y, 8);
        /// Console.WriteLine(expr2);
        /// Console.WriteLine(expr2.Expand());
        /// </code>
        /// Prints
        /// <code>
        /// (x + 3) * (sin(y) + 5)
        /// x * sin(y) + x * 5 + 3 * sin(y) + 15
        /// -----------------------------------
        /// (x + y) ^ 8
        /// y ^ 8 + 8 * x * y ^ 7 + 28 * x ^ 2 * y ^ 6 + 56 * x ^ 3 * y ^ 5 + 70 * x ^ 4 * y ^ 4 + 56 * x ^ 5 * y ^ 3 + 28 * x ^ 6 * y ^ 2 + 8 * x ^ 7 * y + x ^ 8
        /// </code>
        /// </example> 
        public Entity Expand(int level = 2)
        {
            static Entity Expand_(Entity e, int level) =>
                level <= 1
                ? e.Replace(Patterns.ExpandRules)
                : Expand_(e.Replace(Patterns.ExpandRules), level - 1);
            var expChildren = new List<Entity>();
            foreach (var linChild in Sumf.LinearChildren(this))
                if (TreeAnalyzer.SmartExpandOver(linChild, entity => true) is { } exp)
                    expChildren.AddRange(exp);
                else
                    return this; // if one is too complicated, return the current one
            return Expand_(TreeAnalyzer.MultiHangBinary(expChildren, (a, b) => new Sumf(a, b)), level).InnerSimplified;
        }

        /// <summary>
        /// Factorizes an equation trying to eliminate as many power-uses as possible ( e.g. x * 3 + x * y = x * (3 + y) )
        /// </summary>
        /// <param name="level">
        /// The number of iterations (increase this argument if some factor operations are still available)
        /// </param>
        /// <example>
        /// <code>
        /// using System;
        /// using static AngouriMath.MathS;
        /// 
        /// var (x, y) = Var("x", "y");
        /// 
        /// var expr1 = x * y + y + x + 1;
        /// Console.WriteLine(expr1);
        /// Console.WriteLine(expr1.Factorize());
        /// Console.WriteLine("-----------------------------------");
        /// var expr2 = x * y + y + (1 + x);
        /// Console.WriteLine(expr2);
        /// Console.WriteLine(expr2.Factorize());
        /// </code>
        /// Prints
        /// <code>
        /// x * y + y + x + 1
        /// y * (1 + x) + x + 1
        /// -----------------------------------
        /// x * y + y + 1 + x
        /// (1 + x) * (1 + y)
        /// </code>
        /// </example>
        public Entity Factorize(int level = 2) => level <= 1
            ? this.Replace(Patterns.FactorizeRules)
            : this.Replace(Patterns.FactorizeRules).Factorize(level - 1);

        /// <summary>
        /// Simplifies an equation ( e.g. (x - y) * (x + y) -> x^2 - y^2, but 3 * x + y * x = (3 + y) * x )
        /// </summary>
        /// <param name="level">
        /// Increase this argument if you think the equation should be simplified better
        /// </param>
        /// <example>
        /// <code>
        /// using System;
        /// using static AngouriMath.MathS;
        /// 
        /// var (x, y, a) = Var("x", "y", "a");
        /// var expr = Sin(x) + y + a;
        /// Console.WriteLine(expr);
        /// Console.WriteLine(expr.Simplify());
        /// Console.WriteLine("---------------------");
        /// var expr1 = Sin(x - 3) / Tan(x - 3) + Sec(Sqrt(y)) * Cosec(Sqrt(y));
        /// Console.WriteLine(expr1);
        /// Console.WriteLine(expr1.Simplify());
        /// Console.WriteLine("---------------------");
        /// var expr2 = Sin(pi / 3) * 2;
        /// Console.WriteLine(expr2);
        /// Console.WriteLine(expr2.Simplify());
        /// Console.WriteLine("---------------------");
        /// var expr3 = (Pow(x, 3) + 3 * Sqr(x) * y + 3 * x * Sqr(y) + Pow(y, 3)) / (x + y);
        /// Console.WriteLine(expr3);
        /// Console.WriteLine(expr3.Simplify());
        /// Console.WriteLine("---------------------");
        /// var expr4 = Derivative(Sin(Sqr(x * y) + y * x), x);
        /// Console.WriteLine(expr4);
        /// Console.WriteLine(expr4.Simplify());
        /// </code>
        /// Prints
        /// <code>
        /// sin(x) + y + a
        /// sin(x) + a + y
        /// ---------------------
        /// sin(x - 3) / tan(x - 3) + sec(sqrt(y)) * csc(sqrt(y))
        /// 2 * csc(2 * sqrt(y)) + cos(x - 3)
        /// ---------------------
        /// sin(pi / 3) * 2
        /// sqrt(3)
        /// ---------------------
        /// (x ^ 3 + 3 * x ^ 2 * y + 3 * x * y ^ 2 + y ^ 3) / (x + y)
        /// x ^ 2 + 2 * x * y + y ^ 2
        /// ---------------------
        /// derivative(sin((x * y) ^ 2 + y * x), x)
        /// cos((x * y) ^ 2 + x * y) * (2 * x * y ^ 2 + y)
        /// </code>
        /// </example>
        public Entity Simplify(int level = 2) => Simplificator.Simplify(this, level);

        /// <summary>Finds all alternative forms of an expression sorted by their complexity</summary>
        /// <example>
        /// <code>
        /// using System;
        /// using static AngouriMath.MathS;
        /// 
        /// var (x, y, a) = Var("x", "y", "a");
        /// var expr = x * y + y + x + x / y + a * (x + y);
        /// foreach (var alt in expr.Alternate(level: 3))
        ///     Console.WriteLine(alt);
        /// </code>
        /// <code>
        /// a * y + (1 + a + 1 / y + y) * x + y
        /// a * (x + y) + x + x * (y + 1 / y) + y
        /// a * (x + y) + x + x * (1 / y + y) + y
        /// x * y + y + x + x / y + a * (x + y)
        /// a * (x + y) + x + x * y + x / y + y
        /// (x + y) * a + x + x * y + x / y + y
        /// a * (x + y) + x + x * 1 / y + x * y + y
        /// </code>
        /// </example>
        public IEnumerable<Entity> Alternate(int level) => Simplificator.Alternate(this, level);

        /// <summary>
        /// Represents the evaluated value of the given expression
        /// Unlike the result of <see cref="EvalNumerical"/> and
        /// <see cref="EvalBoolean"/>
        /// this is not constrained by any type.
        /// 
        /// It only performs an active operation in the first call,
        /// next time it is free to call it in terms of CPU usage. For
        /// consistency's sake, consider the call of this property
        /// as free as the addressing of a field.
        /// </summary>
        /// <example>
        /// <code>
        /// using System;
        /// using static AngouriMath.MathS;
        /// 
        /// var (x, y) = Var("x", "y");
        /// var expr1 = x + y;
        /// Console.WriteLine(expr1);
        /// Console.WriteLine(expr1.Evaled);
        /// Console.WriteLine(expr1.Evaled.GetType());
        /// Console.WriteLine("-----------------------------");
        /// var expr2 = 5 + x * i;
        /// Console.WriteLine(expr2);
        /// Console.WriteLine(expr2.Evaled);
        /// Console.WriteLine(expr2.Substitute(x, 3).Evaled);
        /// Console.WriteLine(expr2.Substitute(x, 3).Evaled.GetType());
        /// Console.WriteLine("-----------------------------");
        /// var expr3 = GreaterThan(5, 3);
        /// Console.WriteLine(expr3);
        /// Console.WriteLine(expr3.Evaled);
        /// Console.WriteLine(expr3.Evaled.GetType());
        /// </code>
        /// Prints
        /// <code>
        /// x + y
        /// x + y
        /// AngouriMath.Entity+Sumf
        /// -----------------------------
        /// 5 + x * i
        /// 5 + x * i
        /// 5 + 3i
        /// AngouriMath.Entity+Number+Complex
        /// -----------------------------
        /// 5 > 3
        /// True
        /// AngouriMath.Entity+Boolean
        /// </code>
        /// </example>
        public Entity Evaled => evaled.GetValue(static @this => @this.InnerEvalWithCheck(), this);
        private LazyPropertyA<Entity> evaled;

        /// <summary>
        /// Determines whether a given element can be unambiguously used as a number or boolean
        /// </summary>
        /// <example>
        /// <code>
        /// using System;
        /// using static AngouriMath.MathS;
        /// 
        /// var (x, y) = Var("x", "y");
        /// var expr1 = x + y;
        /// Console.WriteLine(expr1.IsConstant);
        /// Console.WriteLine(expr1.Evaled.IsConstant);
        /// Console.WriteLine("-----------------------------");
        /// var expr2 = 5 + x * i;
        /// Console.WriteLine(expr2.IsConstant);
        /// Console.WriteLine(expr2.Substitute(x, 3).IsConstant);
        /// Console.WriteLine("-----------------------------");
        /// var expr3 = GreaterThan(5, 3);
        /// Console.WriteLine(expr3.IsConstant);
        /// Console.WriteLine("-----------------------------");
        /// var expr4 = pi + 0 * e;
        /// Console.WriteLine(expr4.IsConstant);
        /// </code>
        /// Prints
        /// <code>
        /// False
        /// False
        /// -----------------------------
        /// False
        /// True
        /// -----------------------------
        /// True
        /// -----------------------------
        /// True
        /// </code>
        /// </example>
        public bool IsConstant => Evaled is Number.Complex or Boolean || Evaled is Variable v && Variable.ConstantList.ContainsKey(v);
    }
}
