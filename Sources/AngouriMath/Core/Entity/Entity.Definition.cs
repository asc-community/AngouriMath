/* 
 * Copyright (c) 2019-2021 Angouri.
 * AngouriMath is licensed under MIT. 
 * Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
 * Website: https://am.angouri.org.
 */
using System;
using HonkSharp.Laziness;

//[assembly: InternalsVisibleTo("Playground, PublicKey=")]

namespace AngouriMath.Core
{
#pragma warning disable CA1069 // Enums values should not be duplicated
    internal enum Priority
    { 
        KeywordOperation = 0x0000,

        Provided = 10 | KeywordOperation,

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
        /// <inheritdoc/>
        protected abstract Entity[] InitDirectChildren();
        
        /// <summary>
        /// Represents all direct children of a node
        /// </summary>
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
        public abstract Entity Replace(Func<Entity, Entity> func);


        /// <summary>Replaces all <param name="x"/> with <param name="value"/></summary>
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
        public IEnumerable<Variable> Vars => vars.GetValue(static @this => @this.VarsAndConsts.Where(x => !x.IsConstant), this);
        private LazyPropertyA<IEnumerable<Variable>> vars;

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