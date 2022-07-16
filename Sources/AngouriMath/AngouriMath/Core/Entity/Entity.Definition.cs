//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using HonkSharp.Laziness;

//[assembly: InternalsVisibleTo("Playground, PublicKey=")]

namespace AngouriMath.Core
{
#pragma warning disable CA1069 // Enums values should not be duplicated
    internal enum Priority
    { 
        KeywordOperation = 0x0000,

        Lambda   = 10 | KeywordOperation,
        Provided = 20 | KeywordOperation,

        BooleanOperation = 0x1000,

        Impliciation = 10 | BooleanOperation,
        Disjunction  = 30 | BooleanOperation,
        XDisjunction = 30 | BooleanOperation,
        Conjunction  = 50 | BooleanOperation,
        Negation     = 70 | BooleanOperation,

        EqualitySignsOperation = 0x2000,

        Equal          = 10 | EqualitySignsOperation,
        LessThan       = 20 | EqualitySignsOperation,
        GreaterThan    = 20 | EqualitySignsOperation,
        LessOrEqual    = 20 | EqualitySignsOperation,
        GreaterOrEqual = 20 | EqualitySignsOperation,

        SetOperation = 0x3000,

        ContainsIn   = 10 | SetOperation,
        SetMinus     = 20 | SetOperation,
        Union        = 30 | SetOperation,
        Intersection = 40 | SetOperation,

        NumericalOperation = 0x4000,

        Sum = 20       | NumericalOperation,
        Minus = 20     | NumericalOperation,
        Mul = 40       | NumericalOperation,
        Div = 40       | NumericalOperation,
        Pow = 60       | NumericalOperation,
        Factorial = 70 | NumericalOperation,
        Func = 80      | NumericalOperation,


        Leaf      = 100 | NumericalOperation,
    }
#pragma warning restore CA1069 // Enums values should not be duplicated

    /// <summary>
    /// Any class that supports converting to LaTeX format should implement this interface
    /// </summary>
    /// <example>
    /// <code>
    /// using System;
    /// using AngouriMath;
    /// using static AngouriMath.MathS;
    /// 
    /// Entity expr = "sqrt(a) + integral(sin(x), x)";
    /// Console.WriteLine(expr);
    /// Console.WriteLine(expr.Latexise());
    /// Entity expr2 = "a / b ^ limit(sin(x) - cosh(y), x, +oo)";
    /// Console.WriteLine(expr2);
    /// Console.WriteLine(expr2.Latexise());
    /// </code>
    /// Prints
    /// <code>
    /// sqrt(a) + integral(sin(x), x)
    /// \sqrt{a}+\int \left[\sin\left(x\right)\right] dx
    /// a / b ^ limit(sin(x) - (e ^ y + e ^ (-y)) / 2, x, +oo)
    /// \frac{a}{{b}^{\lim_{x\to \infty } \left[\sin\left(x\right)-\frac{{e}^{y}+{e}^{-y}}{2}\right]}}
    /// </code>
    /// </example>
    public interface ILatexiseable
    { 
        /// <summary>
        /// Converts the object to the LaTeX format
        /// That is, a string that can be later displayed and rendered as LaTeX
        /// </summary>
        public string Latexise(); 
    }
}

namespace AngouriMath
{

    /// <summary>
    /// This is the main class in AngouriMath.
    /// Every node, expression, or number is an <see cref="Entity"/>.
    /// However, you cannot create an instance of this class, look for the nested classes instead.
    /// </summary>
    public abstract partial record Entity : ILatexiseable
    {
        /// <summary>
        /// Returns the array of the direct children. Will be
        /// only called once, so no need to cache anything.
        /// </summary>
        protected abstract Entity[] InitDirectChildren();
        
        /// <summary>
        /// Represents all direct children of a node
        /// </summary>
        /// <example>
        /// <code>
        /// using System;
        /// using AngouriMath;
        /// 
        /// Entity frac = "log(sqrt(x), a + b)";
        /// Console.WriteLine(frac);
        /// foreach (var child in frac.DirectChildren)
        ///     Console.WriteLine(child);
        /// </code>
        /// Prints
        /// <code>
        /// log(sqrt(x), a + b)
        /// sqrt(x)
        /// a + b
        /// </code>
        /// </example>
        public IReadOnlyList<Entity> DirectChildren => directChildren.GetValue(static @this => @this.InitDirectChildren(), this);
        private LazyPropertyA<IReadOnlyList<Entity>> directChildren;

        /// <remarks>A depth-first enumeration is required by
        /// <see cref="AngouriMath.Functions.TreeAnalyzer.GetMinimumSubtree"/></remarks>
        /// <summary>
        /// The list of all nodes of the given expression
        /// </summary>
        /// <example>
        /// <code>
        /// Entity expr = "a + b / 2 ^ 3";
        /// Console.WriteLine(string.Join(", ", expr.Nodes));
        /// </code>
        /// Output:
        /// <code>
        /// a + b / 2 ^ 3, a, b / 2 ^ 3, b, 2 ^ 3, 2, 3
        /// </code>
        /// </example>
        public IEnumerable<Entity> Nodes => nodes.GetValue(static @this => @this.DirectChildren.SelectMany(c => c.Nodes).Prepend(@this), this);
        private LazyPropertyA<IEnumerable<Entity>> nodes;

        /// <summary>
        /// Applies the given function to every node starting from the leaves
        /// </summary>
        /// <param name="func">
        /// The delegate that takes the current node as an argument and replaces the current node
        /// with the result of the delegate
        /// </param>
        /// <returns>Processed expression</returns>
        /// <example>
        /// <code>
        /// using System;
        /// using AngouriMath;
        /// using static AngouriMath.MathS;
        /// using static AngouriMath.Entity;
        /// using static AngouriMath.Entity.Number;
        /// 
        /// static Entity MySimplify(Entity expr) => expr switch
        /// {
        ///     Sumf(Powf(Sinf(var e1), Integer(2)), Powf(Cosf(var e2), Integer(2))) when e1 == e2 => 1,
        ///     Sinf(Integer(0)) => 0,
        ///     var other => other
        /// };
        /// 
        /// var x = Var("x");
        /// Entity expr = (Sqr(Sin(x / 3)) + Sqr(Cos(x / 3))) + Sin(0);
        /// Console.WriteLine(expr);
        /// Console.WriteLine(expr.Replace(MySimplify));
        /// </code>
        /// Prints
        /// <code>
        /// sin(x / 3) ^ 2 + cos(x / 3) ^ 2 + 0
        /// 1 + 0
        /// </code>
        /// </example>
        public abstract Entity Replace(Func<Entity, Entity> func);


        /// <summary>Replaces all <param name="x"/> with <param name="value"/></summary>
        /// <returns>A new expression</returns>
        /// <example>
        /// <code>
        /// using System;
        /// using AngouriMath;
        /// using static AngouriMath.MathS;
        /// 
        /// var (x, y, z) = Var("x", "y", "z");
        /// Entity expr = Sin(x);
        /// var substituted = expr.Substitute(x, pi / 3);
        /// 
        /// Console.WriteLine(expr);
        /// Console.WriteLine(substituted);
        /// Console.WriteLine(substituted.Simplify());
        /// 
        /// Console.WriteLine("-------------------------------");
        /// 
        /// var expr2 = Sin(x) + Cos(y + x) + Factorial(z);
        /// 
        /// var substituted2 =
        ///     expr2
        ///         .Substitute(x, 1)
        ///         .Substitute(y, 2)
        ///         .Substitute(z, 3);
        /// 
        /// Console.WriteLine(expr2);
        /// Console.WriteLine(substituted2);
        /// 
        /// Console.WriteLine("-------------------------------");
        /// 
        /// var expr3 = Sin(x + y) + 1 / Sin(x + y);
        /// var substituted3 = expr3.Substitute(Sin(x + y), Cos(x + y));
        /// Console.WriteLine(expr3);
        /// Console.WriteLine(substituted3);
        /// </code>
        /// Prints
        /// <code>
        /// sin(x)
        /// sin(pi / 3)
        /// sqrt(3) / 2
        /// -------------------------------
        /// sin(x) + cos(y + x) + z!
        /// sin(1) + cos(2 + 1) + 3!
        /// -------------------------------
        /// sin(x + y) + 1 / sin(x + y)
        /// cos(x + y) + 1 / cos(x + y)
        /// </code>
        /// </example>
        public virtual Entity Substitute(Entity x, Entity value)
            => this == x ? value : this;

        /// <summary>
        /// Replaces x.x1 with value.v1 and
        /// x.x2 with value.v2
        /// </summary>
        public Entity Substitute((Entity x1, Entity x2) x, (Entity v1, Entity v2) value)
            => Substitute(x.x1, value.v1).Substitute(x.x2, value.v2);

        /// <summary>
        /// Replaces x.x1 with value.v1 and
        /// x.x2 with value.v2 and
        /// x.x3 with value.v3
        /// </summary>
        public Entity Substitute((Entity x1, Entity x2, Entity x3) x, (Entity v1, Entity v2, Entity v3) value)
            => Substitute(x.x1, value.v1).Substitute(x.x2, value.v2).Substitute(x.x3, value.v3);

        /// <summary>
        /// Replaces x.x1 with value.v1 and
        /// x.x2 with value.v2 and
        /// x.x3 with value.v3 and
        /// x.x4 with value.v4
        /// </summary>
        public Entity Substitute((Entity x1, Entity x2, Entity x3, Entity x4) x, (Entity v1, Entity v2, Entity v3, Entity v4) value)
            => Substitute(x.x1, value.v1).Substitute(x.x2, value.v2).Substitute(x.x3, value.v3).Substitute(x.x4, value.v4);

        /// <summary>Replaces all <param name="replacements"/></summary>
        public Entity Substitute<TFrom, TTo>(IReadOnlyDictionary<TFrom, TTo> replacements) where TFrom : Entity where TTo : Entity
        {
            var res = this;
            foreach (var pair in replacements)
                res = res.Substitute(pair.Key, pair.Value);
            return res;
        }

        internal abstract Priority Priority { get; }

        /// <value>
        /// Whether both parts of the complex number are finite
        /// meaning that it could be safely used for calculations
        /// </value>
        public bool IsFinite => isFinite.GetValue(static @this => @this.ThisIsFinite && @this.DirectChildren.All(x => x.IsFinite), this);
        private LazyPropertyA<bool> isFinite;

        /// <summary>
        /// Not NaN and not infinity
        /// </summary>
        protected virtual bool ThisIsFinite => true;       

        /// <value>Number of nodes in tree</value>
        public int Complexity => complexity.GetValue(static @this => 1 + @this.DirectChildren.Sum(x => x.Complexity), this);
        private LazyPropertyA<int> complexity;

        /// <summary>
        /// Set of unique variables, for example 
        /// it extracts <c>`x`</c>, <c>`goose`</c> from <c>(x + 2 * goose) - pi * x</c>
        /// </summary>
        /// <returns>
        /// Set of unique variables excluding mathematical constants
        /// such as <see cref="MathS.pi"/> and <see cref="MathS.e"/>
        /// </returns>
        public IReadOnlyList<Variable> Vars
            => vars.GetValue(static @this
                => @this.VarsAndConsts.Where(x => !x.IsConstant).ToList() /* needed to actually compute it and cache */, this);
        private LazyPropertyA<IReadOnlyList<Variable>> vars;

        /// <summary>
        /// Set of unique variables, for example 
        /// it extracts <c>`x`</c>, <c>`goose`</c>, <c>`pi`</c> from <c>(x + 2 * goose) - pi * x</c>
        /// </summary>
        /// <returns>
        /// Set of unique variables and mathematical constants
        /// such as <see cref="MathS.pi"/> and <see cref="MathS.e"/>
        /// </returns>
        public IReadOnlyCollection<Variable> VarsAndConsts => varsAndConsts.GetValue(
            static @this => new HashSet<Variable>(@this is Variable v ? new[] { v } : @this.DirectChildren.SelectMany(x => x.VarsAndConsts)), this);
        private LazyPropertyA<IReadOnlyCollection<Variable>> varsAndConsts;


        /// <summary>
        /// Returns a set of free variables.
        /// We call a bound variable a variable which is a parameter of some
        /// outer lambda. Then, all other variables are free.
        /// </summary>
        public IReadOnlyCollection<Variable> FreeVariables =>
            freeVariables.GetValue(
                static @this =>
                    @this switch
                    {
                        Lambda(var par, var body)
                            => body.FreeVariables.Where(v => v != par).ToList(),
                        Variable v => new []{ v },
                        _ => new HashSet<Variable>(@this.DirectChildren.SelectMany(c => c.FreeVariables))
                    }
                ,
                this
            );
        private LazyPropertyA<IReadOnlyCollection<Variable>> freeVariables;

        /// <summary>Checks if <paramref name="x"/> is a subnode inside this <see cref="Entity"/> tree.
        /// Optimized for <see cref="Variable"/>.</summary>
        public bool ContainsNode(Entity x) => x is Variable v ? VarsAndConsts.Contains(v) : Nodes.Contains(x);

        /// <summary>
        /// Implicit conversation from string to Entity
        /// </summary>
        /// <param name="expr">The source from which to parse</param>
        public static implicit operator Entity(string expr) => MathS.FromString(expr);

        /// <summary>
        /// Shows how simple the given expression is. The lower - the simpler the expression is.
        /// You might need it to pick the best expression to represent something. Unlike 
        /// <see cref="Complexity"/>, which shows the number of nodes, <see cref="SimplifiedRate"/> 
        /// shows how convenient it is to view the expression. This depends on 
        /// <see cref="MathS.Settings.ComplexityCriteria"/> which can be changed by user.
        /// </summary>
        public double SimplifiedRate => simplifiedRate.GetValue(MathS.Settings.ComplexityCriteria.Value, this);
        private LazyPropertyA<double> simplifiedRate;

        /// <summary>Checks whether the given expression contains variable</summary>
        public bool IsSymbolic => Vars.Any();

        /// <summary>
        /// Checks whether the given expression is a finite constant leaf
        /// </summary>
        public bool IsConstantLeaf => this is Boolean or Number or Set.SpecialSet;
    }
}