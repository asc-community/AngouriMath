/* 
 * Copyright (c) 2019-2021 Angouri.
 * AngouriMath is licensed under MIT. 
 * Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
 * Website: https://am.angouri.org.
 */
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
        public Entity Factorize(int level = 2) => level <= 1
            ? this.Replace(Patterns.FactorizeRules)
            : this.Replace(Patterns.FactorizeRules).Factorize(level - 1);

        /// <summary>
        /// Simplifies an equation ( e.g. (x - y) * (x + y) -> x^2 - y^2, but 3 * x + y * x = (3 + y) * x )
        /// </summary>
        /// <param name="level">
        /// Increase this argument if you think the equation should be simplified better
        /// </param>
        /// <returns></returns>
        public Entity Simplify(int level = 2) => Simplificator.Simplify(this, level);

        /// <summary>Finds all alternative forms of an expression sorted by their complexity</summary>
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
        public Entity Evaled => evaled.GetValue(static @this => @this.InnerEvalWithCheck(), this);
        private LazyPropertyA<Entity> evaled;

        /// <summary>
        /// Determines whether a given element can be unambiguously used as a number or boolean
        /// </summary>
        public bool IsConstant => Evaled is Number.Complex or Boolean || Evaled is Variable v && Variable.ConstantList.ContainsKey(v);
    }
}
