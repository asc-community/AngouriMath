
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AngouriMath.Core;
using AngouriMath.Core.Exceptions;
using AngouriMath.Functions;
using static AngouriMath.Entity.Number;

namespace AngouriMath
{

    partial record Entity
    {        
        public partial record Set : Entity
        {
            #region FiniteSet
            /// <summary>
            /// A finite set is a set whose elements can be counted and enumerated
            /// </summary>
            public partial record FiniteSet : Set, IReadOnlyCollection<Entity>, IEquatable<FiniteSet>
            {
                public IEnumerable<Entity> Elements => elements.Values;

                private readonly Dictionary<Entity, Entity> elements;

                // TODO: refactor
                public override Entity Replace(Func<Entity, Entity> func)
                    => func(Apply(el => el.Replace(func)));

                /// <summary>
                /// Applies a function to every element of a set
                /// and returns a set
                /// </summary>
                /// <param name="func">What we do with each element?</param>
                public FiniteSet Apply(Func<Entity, Entity> func)
                {
                    var hasAnythingChanged = false;
                    List<Entity> newElements = new();
                    foreach (var el in Elements)
                    {
                        var changed = func(el);
                        if (ReferenceEquals(changed, el))
                            hasAnythingChanged = true;
                    }
                    if (hasAnythingChanged)
                        return new FiniteSet(newElements);
                    else
                        return this;
                }

                public override Priority Priority => Priority.Leaf;

                protected override Entity[] InitDirectChildren() => Elements.ToArray();

                private static Dictionary<Entity, Entity> BuildDictionaryFromElements(IEnumerable<Entity> elements, bool noCheck)
                {
                    Dictionary<Entity, Entity> dict = new(elements.Count());
                    foreach (var elem in elements)
                    {
                        if (!noCheck)
                            dict[elem.Evaled] = dict.TryGetValue(elem.Evaled, out var value) ? Simplificator.PickSimplest(elem, value) : elem;
                    }
                    return dict;
                }

                public FiniteSet(IEnumerable<Entity> elements) : this(elements, noCheck: false) { }

                // TODO: can we somehow add { } syntax but avoid inheriting from collection?
                public FiniteSet(params Entity[] elements) : this((IEnumerable<Entity>)elements) { }

                private FiniteSet(IEnumerable<Entity> elements, bool noCheck)
                {
                    this.elements = BuildDictionaryFromElements(elements, noCheck);
                    Count = this.elements.Count;
                }

                public void Deconstruct(out IEnumerable<Entity> elements)
                    => elements = Elements;


                /// <summary> Represents number of entities in the current set </summary>
                public int Count { get; }

                /// <summary>
                /// Used for enumerating. Use "foreach" for iterating over elements
                /// </summary>
                public IEnumerator<Entity> GetEnumerator()
                    => Elements.GetEnumerator();

                IEnumerator IEnumerable.GetEnumerator()
                    => Elements.GetEnumerator();

                internal static FiniteSet Unite(FiniteSet A, FiniteSet B)
                    => new FiniteSet(A.Elements.Concat(B.Elements));

                // It could be written with one chain request, but readability > one line
                internal static FiniteSet Subtract(FiniteSet A, FiniteSet B)
                {
                    var dict = BuildDictionaryFromElements(A.Elements, noCheck: true);
                    foreach (var el in B)
                        dict.Remove(el.Evaled);
                    return new FiniteSet(dict.Values, noCheck: true); // we didn't add anything
                }

                internal static FiniteSet Intersect(FiniteSet A, FiniteSet B)
                {
                    var dict = BuildDictionaryFromElements(A.Elements, noCheck: true);
                    foreach (var el in B)
                        if (!B.ContainsNode(el.Evaled))
                            dict.Remove(el.Evaled);
                    return new FiniteSet(dict.Values, noCheck: true); // we didn't add anything
                }

                public override bool Contains(Entity entity)
                    => elements.ContainsKey(entity.Evaled);

                // TODO: how should we implement GetHashCode?
                // Is it safe to store this hash inside?
                public override int GetHashCode()
                    => Elements.Select(el => el.GetHashCode()).Aggregate((acc, next) => acc + next);

                public virtual bool Equals(FiniteSet? other)
                {
                    if (other is null)
                        return false;
                    if (other.Count != Count)
                        return false;
                    foreach (var pair in elements)
                        if (!other.Contains(pair.Value))
                            return false;
                    return true;
                }
            }
            #endregion

            #region Interval
            /// <summary>
            /// An interval represents all numbres in between two Entities
            /// <see cref="Interval.LeftClosed"/> stands for whether <see cref="Interval.Left"/> is included
            /// <see cref="Interval.RightClosed"/> stands for whether <see cref="Interval.Right"/> is included
            /// </summary>
            public partial record Interval(Entity Left, bool LeftClosed, Entity Right, bool RightClosed) : Set
            {
                public bool IsNumeric => !left.Value.IsNaN && !right.Value.IsNaN;

                // TODO: maybe it's not good to create an object just to lazily initialize and we need to write our own wheel?
                private readonly Lazy<Real> left = new Lazy<Real>(() => Left.EvaluableNumerical && Left.Evaled is Real re ? re : Real.NaN);
                private readonly Lazy<Real> right = new Lazy<Real>(() => Right.EvaluableNumerical && Right.Evaled is Real re ? re : Real.NaN);

                private static bool IsALessThanB(Real A, Real B, bool closed)
                    => A < B || closed && A == B;

                public Interval New(Entity left, Entity right)
                    => ReferenceEquals(Left, left) 
                    && ReferenceEquals(Right, right)
                    ? this : new Interval(left, LeftClosed, right, RightClosed);

                public override Entity Replace(Func<Entity, Entity> func)
                    => func(New(Left.Replace(func), Right.Replace(func)));

                public override bool Contains(Entity entity)
                {
                    if (entity is not Real re)
                        throw new ElementInSetAmbiguousException("The argument should be a real number");
                    if (!IsNumeric)
                        throw new ElementInSetAmbiguousException("The inteval's ends should be real numbers");
                    return IsALessThanB(left.Value, re, LeftClosed) && IsALessThanB(right.Value, re, RightClosed);
                }

                public override Priority Priority => Priority.Leaf;

                protected override Entity[] InitDirectChildren() => new[] { Left, Right };
            }
            #endregion

            #region Conditional Set
            /// <summary>
            /// This is the most abstract set. Technically, you can describe
            /// any set within this one. Formally, you can define set A of
            /// a condition F(x) in the following way: for each element x in 
            /// the Universal x belongs to A if and only if F(x).
            /// </summary>
            public partial record ConditionalSet(Entity Var, Entity Predicate) : Set
            {
                public override Entity Replace(Func<Entity, Entity> func)
                    => func(this);

                public override bool Contains(Entity entity)
                {
                    var substituted = entity.Replace(varCandidate => 
                        varCandidate == Var ? entity : varCandidate).InnerSimplifyWithCheck();
                    if (substituted.EvaluableBoolean)
                        return substituted.EvalBoolean();
                    else
                        throw new ElementInSetAmbiguousException("It is still unclear");
                }

                private Entity New(Entity var, Entity predicate)
                    => ReferenceEquals(Var, var) && ReferenceEquals(Predicate, predicate) ?
                    this : new ConditionalSet(var, predicate);

                public override Priority Priority => Priority.Leaf;

                // TODO: Does conditional set have children?
                protected override Entity[] InitDirectChildren() => new Entity[] { };
            }
            #endregion

            #region Special Set
            /// <summary>
            /// Special set is something that cannot be easily expressed in other
            /// types of sets.
            /// </summary>
            public partial record SpecialSet(Domain SetType) : Set
            {
                public override Entity Replace(Func<Entity, Entity> func)
                    => func(this);

                // TODO: make a more complicated check for more domains
                public override bool Contains(Entity entity)
                    => DomainsFunctional.FitsDomainOrNonNumeric(entity, SetType) && entity is Boolean or Complex;

                // Since there's a very small number of domains, it's wiser to
                // cache them all
                private readonly static Dictionary<Domain, SpecialSet> innerStorage = new();
                public static SpecialSet Create(Domain domain)
                { 
                    if (innerStorage.TryGetValue(domain, out var res))
                        return res;
                    var result = new SpecialSet(domain);
                    innerStorage[domain] = result;
                    return result;
                }

                public override Priority Priority => Priority.Leaf;

                protected override Entity[] InitDirectChildren() => new Entity[] { };
            }
            #endregion  

            #region Union
            /// <summary>
            /// Unites two sets
            /// It is true that an entity is in a union if it is at least in one of union's operands
            /// </summary>
            public partial record Unionf(Entity Left, Entity Right) : Set
            {
                private Unionf New(Entity left, Entity right)
                    => ReferenceEquals(Left, left) && ReferenceEquals(Right, right) ? this : new Unionf(left, right);

                public override Entity Replace(Func<Entity, Entity> func)
                    => func(New(Left.Replace(func), Right.Replace(func)));

                public override bool Contains(Entity entity)
                {
                    if (Left is not Set left || Right is not Set right)
                        throw new ElementInSetAmbiguousException("One of union's operands is not set");
                    return left.Contains(entity) || right.Contains(entity);
                }

                public override Priority Priority => Priority.Union;

                protected override Entity[] InitDirectChildren() => new[] { Left, Right };
            }
            #endregion

            #region Intersection
            /// <summary>
            /// Finds the intersection of two sets
            /// It is true that an entity is in an intersection if it is in both of intersection's operands
            /// </summary>
            public partial record Intersectionf(Entity Left, Entity Right) : Set
            {
                private Intersectionf New(Entity left, Entity right)
                    => ReferenceEquals(Left, left) && ReferenceEquals(Right, right) ? this : new Intersectionf(left, right);

                public override Entity Replace(Func<Entity, Entity> func)
                    => func(New(Left.Replace(func), Right.Replace(func)));

                public override bool Contains(Entity entity)
                {
                    if (Left is not Set left || Right is not Set right)
                        throw new ElementInSetAmbiguousException("One of union's operands is not set");
                    return left.Contains(entity) && right.Contains(entity);
                }

                public override Priority Priority => Priority.Intersection;

                protected override Entity[] InitDirectChildren() => new[] { Left, Right };
            }
            #endregion

            #region Set Minus
            /// <summary>
            /// Finds A & !B
            /// It is true that an entity is in SetMinus if it is in Left but not in Right
            /// </summary>
            public partial record SetMinusf(Entity Left, Entity Right) : Set
            {
                private SetMinusf New(Entity left, Entity right)
                    => ReferenceEquals(Left, left) && ReferenceEquals(Right, right) ? this : new SetMinusf(left, right);

                public override Entity Replace(Func<Entity, Entity> func)
                    => func(New(Left.Replace(func), Right.Replace(func)));

                public override bool Contains(Entity entity)
                {
                    if (Left is not Set left || Right is not Set right)
                        throw new ElementInSetAmbiguousException("One of union's operands is not set");
                    return left.Contains(entity) && !right.Contains(entity);
                }

                public override Priority Priority => Priority.SetMinus;

                protected override Entity[] InitDirectChildren() => new[] { Left, Right };
            }
            #endregion
        }
    }
}
