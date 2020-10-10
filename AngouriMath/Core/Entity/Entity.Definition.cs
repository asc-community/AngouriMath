
/* Copyright (c) 2019-2020 Angourisoft
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,
 * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software
 * is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using AngouriMath.Core;
using AngouriMath.Core.Exceptions;
[assembly: InternalsVisibleTo("Playground")]

namespace AngouriMath.Core
{
    public enum Priority
    { 
        BooleanOperation = 0x0000,

        Impliciation = 10 | BooleanOperation,
        Disjunction  = 30 | BooleanOperation,
        XDisjunction = 30 | BooleanOperation,
        Conjunction  = 50 | BooleanOperation,
        Negation     = 70 | BooleanOperation,

        EqualitySignsOperation = 0x1000,

        Equal          = 10 | EqualitySignsOperation,
        LessThan       = 20 | EqualitySignsOperation,
        GreaterThan    = 20 | EqualitySignsOperation,
        LessOrEqual    = 20 | EqualitySignsOperation,
        GreaterOrEqual = 20 | EqualitySignsOperation,

        NumericalOperation = 0x2000,

        Sum       = 20  | NumericalOperation, 
        Minus     = 20  | NumericalOperation, 
        Mul       = 40  | NumericalOperation, 
        Div       = 40  | NumericalOperation, 
        Pow       = 60  | NumericalOperation, 
        Factorial = 70  | NumericalOperation, 
        Func      = 80  | NumericalOperation, 

        SetOperation = 0x3000,

        ContainsIn   = 10 | SetOperation,
        SetMinus     = 20 | SetOperation,
        Union        = 30 | SetOperation,
        Intersection = 40 | SetOperation,


        Leaf      = 100 | SetOperation,
    }
    public interface ILatexiseable { public string Latexise(); }
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
        private readonly static RecordFieldCache caches = new();

        protected abstract Entity[] InitDirectChildren();
        public IReadOnlyList<Entity> DirectChildren 
            => caches.GetValue(this, cache => cache.directChildren, cache => cache.directChildren = InitDirectChildren());

        /// <remarks>A depth-first enumeration is required by
        /// <see cref="Core.TreeAnalysis.TreeAnalyzer.GetMinimumSubtree(Entity, Variable)"/></remarks>
        public IEnumerable<Entity> Nodes => DirectChildren.SelectMany(c => c.Nodes).Prepend(this);

        public abstract Entity Replace(Func<Entity, Entity> func);
        public Entity Replace(Func<Entity, Entity> func1, Func<Entity, Entity> func2) =>
            Replace(child => func2(func1(child)));
        public Entity Replace(Func<Entity, Entity> func1, Func<Entity, Entity> func2, Func<Entity, Entity> func3) =>
            Replace(child => func3(func2(func1(child))));

        /// <summary>Replaces all <see cref="x"/> with <see cref="value"/></summary>
        public abstract Entity Substitute(Entity x, Entity value);


        // TODO: this function has no performance beneficial anymore, 
        // maybe need to think how it can be improved without defining
        // another virtual method?
        /// <summary>Replaces all <see cref="replacements"/></summary>
        public Entity Substitute<TFrom, TTo>(IReadOnlyDictionary<TFrom, TTo> replacements) where TFrom : Entity where TTo : Entity
        {
            var res = this;
            foreach (var pair in replacements)
                res = res.Substitute(pair.Key, pair.Value);
            return res;
        }

        public abstract Priority Priority { get; }

        /// <value>
        /// Whether both parts of the complex number are finite
        /// meaning that it could be safely used for calculations
        /// </value>
        public bool IsFinite
            => caches.GetValue(this, cache => cache.isFinite, cache => cache.isFinite =
            ThisIsFinite && DirectChildren.All(x => x.IsFinite)) ?? throw new AngouriBugException($"{IsFinite} cannot be null");
        protected virtual bool ThisIsFinite => true;       

        /// <value>Number of nodes in tree</value>
        // TODO: improve measurement of Entity complexity, for example
        // (1 / x ^ 2).Complexity() &lt; (x ^ (-0.5)).Complexity()
        public int Complexity 
            => caches.GetValue(this, 
            cache => cache.complexity,
            cache => cache.complexity = 1 + DirectChildren.Sum(x => x.Complexity)) ?? throw new AngouriBugException("Complexity cannot be null");

        /// <summary>
        /// Set of unique variables, for example 
        /// it extracts <c>`x`</c>, <c>`goose`</c> from <c>(x + 2 * goose) - pi * x</c>
        /// </summary>
        /// <returns>
        /// Set of unique variables excluding mathematical constants
        /// such as <see cref="pi"/> and <see cref="e"/>
        /// </returns>
        public IEnumerable<Variable> Vars => VarsAndConsts.Where(x => !x.IsConstant);
        
        /// <summary>
        /// Set of unique variables, for example 
        /// it extracts <c>`x`</c>, <c>`goose`</c> from <c>(x + 2 * goose) - pi * x</c>
        /// </summary>
        /// <returns>
        /// Set of unique variables and mathematical constants
        /// such as <see cref="pi"/> and <see cref="e"/>
        /// </returns>
        public IReadOnlyCollection<Variable> VarsAndConsts 
            => caches.GetValue(this, cache => cache.vars,
            cache => cache.vars = 
            new HashSet<Variable>(this is Variable v ? new[] { v } : DirectChildren.SelectMany(x => x.VarsAndConsts)));

        /// <summary>Checks if <paramref name="x"/> is a subnode inside this <see cref="Entity"/> tree.
        /// Optimized for <see cref="Variable"/>.</summary>
        public bool ContainsNode(Entity x) => x is Variable v ? VarsAndConsts.Contains(v) : Nodes.Contains(x);

        public static implicit operator Entity(string expr) => MathS.FromString(expr);

        /// <summary>
        /// Shows how simple the given expression is. The lower - the simpler the expression is.
        /// You might need it to pick the best expression to represent something. Unlike 
        /// <see cref="Complexity"/>, which shows the number of nodes, <see cref="SimplifiedRate"/> 
        /// shows how convenient it is to view the expression. This depends on 
        /// <see cref="MathS.Settings.ComplexityCriteria"/> which can be changed by user.
        /// </summary>
        public int SimplifiedRate => caches.GetValue(this, cache => cache.simplifiedRate, cache => cache.simplifiedRate = MathS.Settings.ComplexityCriteria.Value(this)) ?? throw new AngouriBugException("Sim cannot be null");

        protected virtual bool PrintMembers(StringBuilder builder)
        {
            builder.Append(Stringize());
            return false;
        }
    }
}