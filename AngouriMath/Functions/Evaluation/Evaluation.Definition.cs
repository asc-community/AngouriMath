/*
 * Copyright (c) 2019-2020 Angourisoft
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using System.Collections.Generic;
using AngouriMath.Functions;
using System;
using AngouriMath.Core;
using static AngouriMath.Entity.Number;
using System.Linq;

namespace AngouriMath
{
    public abstract partial record Entity
    {
        /// <summary>
        /// This should NOT be called inside itself
        /// </summary>
        protected abstract Entity InnerSimplify();

        /// <summary>
        /// This is the result of naive simplifications. In other 
        /// symbolic algebra systems it is called "Automatic simplification"
        /// </summary>
        public Entity InnerSimplified 
            => Caches.GetValue(this, cache => cache.innerSimplified, cache => cache.innerSimplified = InnerSimplifyWithCheck());

        /// <summary>
        /// Make sure you call this function inside of <see cref="InnerSimplify"/>
        /// </summary>
        internal Entity InnerSimplifyWithCheck()
        {
            var innerSimplified = InnerSimplify();
            if (DomainsFunctional.FitsDomainOrNonNumeric(innerSimplified, Codomain))
                return innerSimplified;
            else
                return this;
        }

        /// <summary>
        /// This should NOT be called inside itself
        /// </summary>
        protected abstract Entity InnerEval();
        
        /// <summary>
        /// Make sure you call this function inside of <see cref="InnerEval"/>
        /// </summary>
        protected Entity InnerEvalWithCheck()
        {
            var innerEvaled = InnerEval();
            if (DomainsFunctional.FitsDomainOrNonNumeric(innerEvaled, Codomain))
                return innerEvaled;
            else
                return MathS.NaN;
        }

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
        /// Unlike the result of <see cref="EvalNumerical"/>,
        /// <see cref="EvalBoolean"/> and <see cref="EvalTensor"/>,
        /// this is not constrained by any type
        /// (cached value)
        /// </summary>
        public Entity Evaled 
            => Caches.GetValue(this, cache => cache.innerEvaled, cache => cache.innerEvaled = InnerEvalWithCheck());

        /// <summary>
        /// Whether the expression can be collapsed to a tensor
        /// </summary>
        public bool IsTensoric => Nodes.Any(c => c is Tensor);

        /// <summary>
        /// Evaluates the entire expression into a <see cref="Tensor"/> if possible
        /// ( x y ) + 1 => ( x+1 y+1 )
        /// 
        /// ( 1 2 ) + ( 3 4 ) => ( 4 6 ) vectors pointwise
        /// 
        ///              (( 3 )
        /// (( 1 2 3 )) x ( 4 ) => (( 26 )) Matrices dot product
        ///               ( 5 ))
        ///               
        /// ( 1 2 ) x ( 1 3 ) => ( 1 6 ) Vectors pointwise
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this entity cannot be represented as a <see cref="Tensor"/>.
        /// <see cref="IsTensoric"/> should be used to check beforehand.
        /// </exception>
        public Tensor EvalTensor() =>
            Evaled is Tensor value ? value :
                throw new InvalidOperationException
                    ($"Result cannot be represented as a {nameof(Tensor)}! Check the type of {nameof(Evaled)} beforehand.");

        /// <summary>
        /// Determines whether a given element can be unambiguously used as a number or boolean
        /// </summary>
        public bool IsConstant => Evaled is Complex or Boolean || Evaled is Variable v && Variable.ConstantList.ContainsKey(v);
    }
}
