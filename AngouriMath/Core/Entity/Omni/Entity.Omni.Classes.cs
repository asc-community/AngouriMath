/*
 * Copyright (c) 2019-2020 Angourisoft
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AngouriMath.Core;
using AngouriMath.Core.Exceptions;
using AngouriMath.Core.Sets;
using AngouriMath.Functions;
using AngouriMath.Functions.Boolean;
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
                /// <summary>
                /// The IEnumerable of elements of a finite set
                /// Which is readonly
                /// </summary>
                public IEnumerable<Entity> Elements => elements.Values;

                private readonly Dictionary<Entity, Entity> elements;

                // TODO: refactor
                /// <inheritdoc/>
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
                    FiniteSetBuilder newElements = new();
                    foreach (var el in Elements)
                    {
                        var changed = func(el);
                        if (!ReferenceEquals(changed, el))
                            hasAnythingChanged = true;
                        newElements.Add(changed);
                    }
                    if (hasAnythingChanged)
                        return newElements.ToFiniteSet();
                    else
                        return this;
                }

                internal override Priority Priority => Priority.Leaf;

                /// <inheritdoc/>
                protected override Entity[] InitDirectChildren() => Elements.ToArray();

                private static Dictionary<Entity, Entity> BuildDictionaryFromElements(IEnumerable<Entity> elements, bool noCheck)
                {
                    Dictionary<Entity, Entity> dict = new(elements.Count());
                    foreach (var elem in elements)
                    {
                        if (noCheck)
                            dict[elem.Evaled] = elem;
                        else
                            dict[elem.Evaled] = dict.TryGetValue(elem.Evaled, out var value) ? Simplificator.PickSimplest(elem, value) : elem;
                    }
                    return dict;
                }

                /// <summary>
                /// Constructor of a finite set
                /// Use <see cref="Empty"/> to create an empty set
                /// </summary>
                public FiniteSet(IEnumerable<Entity> elements) : this(elements, noCheck: false) { }

                // TODO: can we somehow add { } syntax but avoid inheriting from collection?
                /// <summary>
                /// Constructor of a finite set
                /// Use <see cref="Empty"/> to create an empty set
                /// </summary>
                public FiniteSet(params Entity[] elements) : this((IEnumerable<Entity>)elements) { }

                private FiniteSet(IEnumerable<Entity> elements, bool noCheck)
                {
                    this.elements = BuildDictionaryFromElements(elements, noCheck);
                    Count = this.elements.Count;
                }

                /// <summary>
                /// Deconstructs as record
                /// </summary>
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
                    foreach (var el in A.elements)
                        if (!B.Contains(el.Key))
                            dict.Remove(el.Key);
                    return new FiniteSet(dict.Values, noCheck: true); // we didn't add anything
                }

                /// <inheritdoc/>
                public override bool TryContains(Entity entity, out bool contains)
                {
                    contains = elements.ContainsKey(entity.Evaled);
                    // a in { 2, 3 } is false
                    // 4 in { a, 3 } is false
                    // TODO: should we return false when there are symbolic expressions?
                    return true;
                }

                // TODO: how should we implement GetHashCode?
                // Is it safe to store this hash inside?
                /// <inheritdoc/>
                public override int GetHashCode()
                    => IsSetEmpty ? 0 : Elements.Select(el => el.GetHashCode()).Aggregate((acc, next) => acc + next);

                /// <summary>
                /// Checks that two FiniteSets are equal
                /// If one is not FiniteSet, the method returns false
                /// </summary>
                public virtual bool Equals(FiniteSet other)
                {
                    if (other is null)
                        return false;
                    if (other.Count != Count)
                        return false;
                    foreach (var pair in elements)
                        if (!other.Contains(pair.Key))
                            return false;
                    return true;
                }

                /// <summary>
                /// Finds such a set that only contains all subsets of the given set
                /// </summary>
                public FiniteSet GetPowerSet() => powerSet ??= PowerSetWorkout();
                
                private FiniteSet? powerSet;

                private FiniteSet PowerSetWorkout()
                {
                    var sets = new List<Entity>();
                    var state = new bool[Count];
                    var elements = Elements.ToArray();
                    do
                    {
                        var innerSetElements = new List<Entity>();
                        for (int i = 0; i < Count; i++)
                            if (state[i])
                                innerSetElements.Add(elements[i]);
                        sets.Add(new FiniteSet(innerSetElements, noCheck: true));
                    }
                    while (BooleanSolver.Next(state));
                    return new FiniteSet(sets, noCheck: true);
                }

                /// <summary>
                /// Safely checks whether this is a subset of the argument,
                /// if successful, returns true AND stores the result in <paramref name="isSub"/>
                /// </summary>
                /// <returns>Whether it is possible to determine</returns>
                public bool TryIsSubsetOf(FiniteSet superSet, out bool isSub)
                {
                    isSub = false;
                    foreach (var el in this)
                        if (!superSet.TryContains(el, out var contains))
                            return false;
                        else if (!contains)
                            return true;

                    isSub = true;
                    return true;
                }

                /// <inheritdoc/>
                public override bool IsSetFinite => true;

                /// <inheritdoc/>
                public override bool IsSetEmpty => Count == 0;
            }
            #endregion

#pragma warning disable CS1591 // TODO: it's only for records' parameters! Remove it once you can document records parameters

            #region Interval
            /// <summary>
            /// An interval represents all numbres in between two Entities
            /// <see cref="Interval.LeftClosed"/> stands for whether <see cref="Interval.Left"/> is included
            /// <see cref="Interval.RightClosed"/> stands for whether <see cref="Interval.Right"/> is included
            /// </summary>
            public partial record Interval(Entity Left, bool LeftClosed, Entity Right, bool RightClosed) : Set, IEquatable<Interval>
            {
                /// <summary>
                /// Checks whether the interval's ends are both numerical (convenient for some evaluations)
                /// </summary>
                public bool IsNumeric => !left.Value.IsNaN && !right.Value.IsNaN;

                // TODO: maybe it's not good to create an object just to lazily initialize and we need to write our own wheel?
                private readonly Lazy<Real> left = new Lazy<Real>(() => Left.EvaluableNumerical && Left.Evaled is Real re ? re : Real.NaN);
                private readonly Lazy<Real> right = new Lazy<Real>(() => Right.EvaluableNumerical && Right.Evaled is Real re ? re : Real.NaN);

                private static bool IsALessThanB(Real A, Real B, bool closed)
                    => A < B || closed && A == B;

                internal Interval New(Entity left, Entity right)
                    => ReferenceEquals(Left, left) 
                    && ReferenceEquals(Right, right)
                    ? this : new Interval(left, LeftClosed, right, RightClosed);

                internal Interval New(Entity left, bool leftClosed, Entity right, bool rightClosed)
                    => ReferenceEquals(Left, left) && ReferenceEquals(Right, right)
                    && LeftClosed == leftClosed && RightClosed == rightClosed
                    ? this : new Interval(left, leftClosed, right, rightClosed);

                /// <inheritdoc/>
                public override Entity Replace(Func<Entity, Entity> func)
                    => func(New(Left.Replace(func), Right.Replace(func)));

                /// <inheritdoc/>
                public override bool TryContains(Entity entity, out bool contains)
                {
                    contains = false;
                    if (entity is Complex and not Real)
                    {
                        contains = false;
                        return true;
                    }
                    if (Left == Right && !(LeftClosed && RightClosed))
                    {
                        contains = false;
                        return true;
                    }
                    if (!IsNumeric)
                    {
                        if (entity == Left)
                        {
                            contains = LeftClosed;
                            return true;
                        }
                        if (entity == Right)
                        {
                            contains = RightClosed;
                            return true;
                        }
                    }
                    if (entity is not Real re)
                        return false;
                    if (!IsNumeric)
                        return false;
                    contains = IsALessThanB(left.Value, re, LeftClosed) && IsALessThanB(re, right.Value, RightClosed);
                    return true;
                }

                internal override Priority Priority => Priority.Leaf;

                /// <inheritdoc/>
                protected override Entity[] InitDirectChildren() => new[] { Left, Right };

                /// <summary>
                /// Checks that two intervals are equal
                /// If one is not interval, false is returned
                /// </summary>
                public virtual bool Equals(Interval other)
                    => other is not null && (Left == other.Left
                        && Right == other.Right
                        && LeftClosed == other.LeftClosed
                        && RightClosed == other.RightClosed);

                /// <inheritdoc/>
                public override int GetHashCode()
                    => Left.GetHashCode() + LeftClosed.GetHashCode() + Right.GetHashCode() + RightClosed.GetHashCode();

                /// <inheritdoc/>
                public override bool IsSetFinite => false;

                /// <inheritdoc/>
                public override bool IsSetEmpty => false;
            }
            #endregion

            #region Conditional Set
            /// <summary>
            /// This is the most abstract set. Technically, you can describe
            /// any set within this one. Formally, you can define set A of
            /// a condition F(x) in the following way: for each element x in 
            /// the Universal x belongs to A if and only if F(x).
            /// </summary>
            public partial record ConditionalSet(Entity Var, Entity Predicate) : Set, IEquatable<ConditionalSet>
            {
                /// <inheritdoc/>
                public override Entity Replace(Func<Entity, Entity> func)
                    => func(New(Var, Predicate.Replace(func)));

                /// <inheritdoc/>
                public override bool TryContains(Entity entity, out bool contains)
                {
                    contains = false;
                    var substituted = Predicate.Substitute(Var, entity);
                    substituted = substituted.InnerSimplified;
                    if (substituted.EvaluableBoolean)
                    {
                        contains = substituted.EvalBoolean();
                        return true;
                    }
                    else
                        return false;
                }

                internal Entity New(Entity var, Entity predicate)
                    => ReferenceEquals(Var, var) && ReferenceEquals(Predicate, predicate) ?
                    this : new ConditionalSet(var, predicate);

                internal override Priority Priority => Priority.Leaf;

                // TODO: Does conditional set have children?
                /// <inheritdoc/>
                protected override Entity[] InitDirectChildren() => new[] { Predicate };

                // TODO:
                /// <inheritdoc/>
                public override bool IsSetFinite => false;
                /// <inheritdoc/>
                public override bool IsSetEmpty => Predicate.Evaled == Boolean.False;

                /// <summary>
                /// Compares two ConditionalSets
                /// If one is not CSet, false is returned
                /// </summary>
                public virtual bool Equals(ConditionalSet other)
                {
                    if (other is null) // invalid cast
                        return false;
                    var (one, two) = SetOperators.MergeToOneVariable(this, other);
                    return one.Predicate == two.Predicate;
                }


                private readonly static Variable universalVoidConstant = Variable.CreateVariableUnchecked("%");
                /// <inheritdoc/>
                public override int GetHashCode()
                    // TODO: might not always work, requires testing
                    => Predicate.Substitute(Var, universalVoidConstant).GetHashCode();
            }
            #endregion

            #region Special Set
            /// <summary>
            /// Special set is something that cannot be easily expressed in other
            /// types of sets.
            /// </summary>
            public abstract partial record SpecialSet : Set
            {
                /// <inheritdoc/>
                public override Entity Replace(Func<Entity, Entity> func)
                    => func(this);

                // Since there's a very small number of domains, it's wiser to
                // cache them all
                private readonly static Dictionary<Domain, SpecialSet> innerStorage = new();

                /// <summary>
                /// Creates an instance of special set from a domain
                /// </summary>
                public static SpecialSet Create(Domain domain)
                { 
                    if (innerStorage.TryGetValue(domain, out var res))
                        return res;
                    
                    SpecialSet result = domain switch
                    {
                        Domain.Boolean => new Booleans(),
                        Domain.Integer => new Integers(),
                        Domain.Rational => new Rationals(),
                        Domain.Real => new Reals(),
                        Domain.Complex => new Complexes(),
                        _ => throw new AngouriBugException("The given domain is not presented in those possible")
                    };
                    innerStorage[domain] = result;
                    return result;
                }

                internal static Domain ToDomain(string domain)
                    => domain switch
                    {
                        "BB" or "Booleans" => Domain.Boolean,
                        "ZZ" or "Integers" => Domain.Integer,
                        "QQ" or "Rationals" => Domain.Rational,
                        "RR" or "Reals" => Domain.Real,
                        "CC" or "Complexes" => Domain.Complex,
                        _ => throw new AngouriBugException("The given domain is not presented in those possible")
                    };

                /// <summary>
                /// Creates an instance of special set from a string
                /// </summary>
                public static SpecialSet Create(string domain)
                    => Create(ToDomain(domain));


                internal override Priority Priority => Priority.Leaf;

                /// <inheritdoc/>
                protected override Entity[] InitDirectChildren() => Array.Empty<Entity>();

                /// <inheritdoc/>
                public override bool IsSetFinite => false;

                /// <inheritdoc/>
                public override bool IsSetEmpty => false;

                /// <summary>Checks whether the given element</summary>
                public abstract bool MayContain(Entity entity);

                /// <inheritdoc/>
                public override bool TryContains(Entity entity, out bool contains)
                {
                    contains = false;
                    if (entity.Evaled.IsSymbolic)
                        return false;
                    contains = MayContain(entity);
                    return true;
                }

                internal abstract Domain ToDomain();

                /// <summary>
                /// A set of all booleans
                /// </summary>
                public sealed partial record Booleans : SpecialSet
                {
                    /// <inheritdoc/>
                    public override bool MayContain(Entity entity)
                        => entity.Evaled is Boolean || entity.Evaled.IsSymbolic;
                    internal override Domain ToDomain() => Domain.Boolean;
                }

                /// <summary>
                /// A set of all integers
                /// </summary>
                public sealed partial record Integers : SpecialSet
                {
                    /// <inheritdoc/>
                    public override bool MayContain(Entity entity)
                        => entity.Evaled is Integer || entity.Evaled.IsSymbolic;
                    internal override Domain ToDomain() => Domain.Integer;
                }

                /// <summary>
                /// A set of all rational numbers
                /// </summary>
                public sealed partial record Rationals : SpecialSet
                {
                    /// <inheritdoc/>
                    public override bool MayContain(Entity entity)
                        => entity.Evaled is Rational || entity.Evaled.IsSymbolic;
                    internal override Domain ToDomain() => Domain.Rational;
                }

                /// <summary>
                /// A set of all real numbers
                /// </summary>
                public sealed partial record Reals : SpecialSet
                {
                    /// <inheritdoc/>
                    public override bool MayContain(Entity entity)
                        => entity.Evaled is Real || entity.Evaled.IsSymbolic;
                    internal override Domain ToDomain() => Domain.Real;
                }

                /// <summary>
                /// A set of all complex numbers
                /// </summary>
                public sealed partial record Complexes : SpecialSet
                {
                    /// <inheritdoc/>
                    public override bool MayContain(Entity entity)
                        => entity.Evaled is Complex || entity.Evaled.IsSymbolic;
                    internal override Domain ToDomain() => Domain.Complex;
                }
            }
            #endregion  

            #region Union
            /// <summary>
            /// Unites two sets
            /// It is true that an entity is in a union if it is at least in one of union's operands
            /// </summary>
            public sealed partial record Unionf(Entity Left, Entity Right) : Set
            {
                private Unionf New(Entity left, Entity right)
                    => ReferenceEquals(Left, left) && ReferenceEquals(Right, right) ? this : new Unionf(left, right);

                /// <inheritdoc/>
                public override Entity Replace(Func<Entity, Entity> func)
                    => func(New(Left.Replace(func), Right.Replace(func)));

                /// <inheritdoc/>
                public override bool TryContains(Entity entity, out bool contains)
                {
                    contains = false;
                    if (Left is not Set left || Right is not Set right)
                        return false;
                    if (left.TryContains(entity, out var leftContains) && right.TryContains(entity, out var rightContains))
                    {
                        contains = leftContains || rightContains;
                        return true;
                    }
                    return false;
                }

                internal override Priority Priority => Priority.Union;

                /// <inheritdoc/>
                protected override Entity[] InitDirectChildren() => new[] { Left, Right };

                /// <inheritdoc/>
                public override bool IsSetFinite => caches.GetValue(this,
                    cache => cache.isSetFinite, cache => cache.isSetFinite =
                    Left is FiniteSet finite1 && Right is FiniteSet finite2
                    && finite1.IsSetFinite && finite2.IsSetFinite) ?? throw new AngouriBugException("isSetFinite cannot be null");

                /// <inheritdoc/>
                public override bool IsSetEmpty => caches.GetValue(this,
                    cache => cache.isSetEmpty, cache => cache.isSetEmpty =
                    Left is FiniteSet finite1 && Right is FiniteSet finite2
                    && finite1.IsSetEmpty && finite2.IsSetEmpty) ?? throw new AngouriBugException("isSetEmpty cannot be null");
            }
            #endregion

            #region Intersection
            /// <summary>
            /// Finds the intersection of two sets
            /// It is true that an entity is in an intersection if it is in both of intersection's operands
            /// </summary>
            public sealed partial record Intersectionf(Entity Left, Entity Right) : Set
            {
                private Intersectionf New(Entity left, Entity right)
                    => ReferenceEquals(Left, left) && ReferenceEquals(Right, right) ? this : new Intersectionf(left, right);

                /// <inheritdoc/>
                public override Entity Replace(Func<Entity, Entity> func)
                    => func(New(Left.Replace(func), Right.Replace(func)));

                /// <inheritdoc/>
                public override bool TryContains(Entity entity, out bool contains)
                {
                    contains = false;
                    if (Left is not Set left || Right is not Set right)
                        return false;
                    if (left.TryContains(entity, out var leftContains) && right.TryContains(entity, out var rightContains))
                    {
                        contains = leftContains && rightContains;
                        return true;
                    }
                    return false;
                }

                internal override Priority Priority => Priority.Intersection;

                /// <inheritdoc/>
                protected override Entity[] InitDirectChildren() => new[] { Left, Right };

                /// <inheritdoc/>
                public override bool IsSetFinite => caches.GetValue(this,
                    cache => cache.isSetFinite, cache => cache.isSetFinite = Left is FiniteSet finite1 && Right is FiniteSet finite2
                    && (finite1.IsSetFinite || finite2.IsSetFinite)) ?? throw new AngouriBugException("isSetFinite cannot be null");

                /// <inheritdoc/>
                public override bool IsSetEmpty => caches.GetValue(this,
                    cache => cache.isSetEmpty, cache => cache.isSetEmpty =
                    Left is FiniteSet finite1 && Right is FiniteSet finite2
                    && (finite1.IsSetEmpty || finite2.IsSetEmpty)) ?? throw new AngouriBugException("isSetEmpty cannot be null");
            }
            #endregion

            #region Set Minus
            /// <summary>
            /// Finds A &amp; !B
            /// It is true that an entity is in SetMinus if it is in Left but not in Right
            /// </summary>
            public sealed partial record SetMinusf(Entity Left, Entity Right) : Set
            {
                private SetMinusf New(Entity left, Entity right)
                    => ReferenceEquals(Left, left) && ReferenceEquals(Right, right) ? this : new SetMinusf(left, right);

                /// <inheritdoc/>
                public override Entity Replace(Func<Entity, Entity> func)
                    => func(New(Left.Replace(func), Right.Replace(func)));

                /// <inheritdoc/>
                public override bool TryContains(Entity entity, out bool contains)
                {
                    contains = false;
                    if (Left is not Set left || Right is not Set right)
                        return false;
                    if (left.TryContains(entity, out var leftContains) && right.TryContains(entity, out var rightContains))
                    {
                        contains = leftContains && !rightContains;
                        return true;
                    }
                    return false;
                }

                internal override Priority Priority => Priority.SetMinus;

                /// <inheritdoc/>
                protected override Entity[] InitDirectChildren() => new[] { Left, Right };

                /// <inheritdoc/>
                public override bool IsSetFinite => caches.GetValue(this,
                    cache => cache.isSetFinite, cache => cache.isSetFinite =
                    Left is FiniteSet finite1 && Right is FiniteSet && finite1.IsSetFinite) ?? throw new AngouriBugException("isSetFinite cannot be null");

                /// <inheritdoc/>
                public override bool IsSetEmpty => caches.GetValue(this,
                    cache => cache.isSetEmpty, cache => cache.isSetEmpty =
                    Left is FiniteSet finite1 && Right is FiniteSet finite2
                    && (finite1.IsSetEmpty || finite1 == finite2)) ?? throw new AngouriBugException("isSetEmpty cannot be null");
            }
            #endregion
        }
#pragma warning restore CS1591 // TODO: it's only for records' parameters! Remove it once you can document records parameters
    }
}
