//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Collections;
using AngouriMath.Core.HashCode;
using AngouriMath.Core.Exceptions;
using AngouriMath.Core.Sets;
using AngouriMath.Functions.Boolean;
using HonkSharp.Laziness;
using Complex = AngouriMath.Entity.Number.Complex;
using System.Linq.Expressions;

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
            public sealed partial record FiniteSet : Set, IReadOnlyCollection<Entity>, IEquatable<FiniteSet?>
            {
                /// <summary>
                /// The IEnumerable of elements of a finite set
                /// Which is readonly
                /// </summary>
                public IEnumerable<Entity> Elements => elements.Values;

                private readonly Dictionary<Entity, Entity> elements;

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
                    var enumerable = elements as Entity[] ?? elements.ToArray();
                    Dictionary<Entity, Entity> dict = new(enumerable.Length);
                    foreach (var elem in enumerable)
                    {
                        if (elem == MathS.NaN)
                            continue;
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

                /// <inheritdoc/>
                public override int GetHashCode()
                    => hashCode.GetValue(static @this =>
                        @this.Elements.OrderBy(el => el.GetHashCode()).HashCodeOfSequence(HashCodeFunctional.HashCodeShifts.FiniteSet), 
                        this);
                private LazyPropertyA<int> hashCode;

                /// <summary>
                /// Checks that two FiniteSets are equal
                /// If one is not FiniteSet, the method returns false
                /// </summary>
                public bool Equals(FiniteSet? other)
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

                /// <inheritdoc/>
                public override Set Filter(Entity predicate, Variable over) => Apply(c => c.Provided(predicate.Substitute(over, c)));
            }
            #endregion

#pragma warning disable CS1591 // TODO: it's only for records' parameters! Remove it once you can document records parameters

            #region Interval
            /// <summary>
            /// An interval represents all numbres in between two Entities
            /// <see cref="Interval.LeftClosed"/> stands for whether <see cref="Interval.Left"/> is included
            /// <see cref="Interval.RightClosed"/> stands for whether <see cref="Interval.Right"/> is included
            /// </summary>
            public sealed partial record Interval(Entity Left, bool LeftClosed, Entity Right, bool RightClosed) : Set, IEquatable<Interval?>
            {
                /// <summary>
                /// Checks whether the interval's ends are both numerical (convenient for some evaluations)
                /// </summary>
                public bool IsNumeric => !LeftReal.IsNaN && !RightReal.IsNaN;

                private Real LeftReal => fLeft.GetValue(static @this => @this.Left.EvaluableNumerical && @this.Left.Evaled is Real re ? re : Real.NaN, this);
                private LazyPropertyA<Real> fLeft;
                private Real RightReal => fRight.GetValue(static @this => @this.Right.EvaluableNumerical && @this.Right.Evaled is Real re ? re : Real.NaN, this);
                private LazyPropertyA<Real> fRight;

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
                    entity = entity.Evaled;
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
                    contains = IsALessThanB(LeftReal, re, LeftClosed) && IsALessThanB(re, RightReal, RightClosed);
                    return true;
                }

                internal override Priority Priority => Priority.Leaf;

                /// <inheritdoc/>
                protected override Entity[] InitDirectChildren() => new[] { Left, Right };

                /// <summary>
                /// Checks that two intervals are equal
                /// If one is not interval, false is returned
                /// </summary>
                public bool Equals(Interval? other)
                    => other is not null && (Left == other.Left
                        && Right == other.Right
                        && LeftClosed == other.LeftClosed
                        && RightClosed == other.RightClosed);

                /// <inheritdoc/>
                public override int GetHashCode()
                    => (Left, LeftClosed, Right, RightClosed).GetHashCode();

                /// <inheritdoc/>
                public override bool IsSetFinite => false;

                /// <inheritdoc/>
                public override bool IsSetEmpty => false;

                /// <inheritdoc/>
                public override Set Filter(Entity predicate, Variable over) => new ConditionalSet(over, predicate & over.In(this));
            }
            #endregion

            #region Conditional Set
            /// <summary>
            /// This is the most abstract set. Technically, you can describe
            /// any set within this one. Formally, you can define set A of
            /// a condition F(x) in the following way: for each element x in 
            /// the Universal x belongs to A if and only if F(x).
            /// </summary>
            /// <example>
            /// <code>
            /// using AngouriMath;
            /// using System;
            /// using static AngouriMath.Entity.Set;
            /// using static AngouriMath.MathS;
            /// using static AngouriMath.MathS.Sets;
            /// 
            /// var set1 = Finite(1, 2, 3);
            /// var set2 = Finite(2, 3, 4);
            /// var set3 = MathS.Interval(-6, 2);
            /// var set4 = new ConditionalSet("x", "100 &gt; x2 &gt; 81");
            /// Console.WriteLine(Union(set1, set2));
            /// Console.WriteLine(Union(set1, set2).Simplify());
            /// Console.WriteLine("----------------------");
            /// Console.WriteLine(Union(set1, set3));
            /// Console.WriteLine(Union(set1, set3).Simplify());
            /// Console.WriteLine("----------------------");
            /// Console.WriteLine(Union(set1, set4));
            /// Console.WriteLine(ElementInSet(3, Union(set1, set4)));
            /// Console.WriteLine(ElementInSet(3, Union(set1, set4)).Simplify());
            /// Console.WriteLine(ElementInSet(4, Union(set1, set4)));
            /// Console.WriteLine(ElementInSet(4, Union(set1, set4)).Simplify());
            /// Console.WriteLine(ElementInSet(9.5, Union(set1, set4)));
            /// Console.WriteLine(ElementInSet(9.5, Union(set1, set4)).Simplify());
            /// Console.WriteLine("----------------------");
            /// Console.WriteLine(Intersection(set1, set2));
            /// Console.WriteLine(Intersection(set1, set2).Simplify());
            /// Console.WriteLine("----------------------");
            /// Console.WriteLine(Intersection(set2, set3));
            /// Console.WriteLine(Intersection(set2, set3).Simplify());
            /// Console.WriteLine("----------------------");
            /// var set5 = MathS.Interval(-3, 11);
            /// Console.WriteLine(Intersection(set3, set5));
            /// Console.WriteLine(Intersection(set3, set5).Simplify());
            /// Console.WriteLine(Union(set3, set5));
            /// Console.WriteLine(Union(set3, set5).Simplify());
            /// Console.WriteLine(SetSubtraction(set3, set5));
            /// Console.WriteLine(SetSubtraction(set3, set5).Simplify());
            /// Console.WriteLine("----------------------");
            /// Entity syntax1 = @"{ 1, 2, 3 } /\ { 2, 3, 4 }";
            /// Console.WriteLine(syntax1);
            /// Console.WriteLine(syntax1.Simplify());
            /// Console.WriteLine("----------------------");
            /// Entity syntax2 = @"5 in ([1; +oo) \/ { x : x &lt; -4 })";
            /// Console.WriteLine(syntax2);
            /// Console.WriteLine(syntax2.Simplify());
            /// Console.WriteLine("----------------------");
            /// Console.WriteLine(Intersection(Finite(pi, e, 6, 5.5m, 1 + 3 * i), Q));
            /// Console.WriteLine(Intersection(Finite(pi, e, 6, 5.5m, 1 + 3 * i), Q).Simplify());
            /// Console.WriteLine(Intersection(Finite(pi, e, 6, 5.5m, 1 + 3 * i), R));
            /// Console.WriteLine(Intersection(Finite(pi, e, 6, 5.5m, 1 + 3 * i), R).Simplify());
            /// Console.WriteLine(Intersection(Finite(pi, e, 6, 5.5m, 1 + 3 * i), C));
            /// Console.WriteLine(Intersection(Finite(pi, e, 6, 5.5m, 1 + 3 * i), C).Simplify());
            /// </code>
            /// Prints
            /// <code>
            /// { 1, 2, 3 } \/ { 2, 3, 4 }
            /// { 1, 2, 3, 4 }
            /// ----------------------
            /// { 1, 2, 3 } \/ [-6; 2]
            /// { 3 } \/ [-6; 2]
            /// ----------------------
            /// { 1, 2, 3 } \/ { x : 100 &gt; x ^ 2 and x ^ 2 &gt; 81 }
            /// 3 in { 1, 2, 3 } \/ { x : 100 &gt; x ^ 2 and x ^ 2 &gt; 81 }
            /// True
            /// 4 in { 1, 2, 3 } \/ { x : 100 &gt; x ^ 2 and x ^ 2 &gt; 81 }
            /// False
            /// 19/2 in { 1, 2, 3 } \/ { x : 100 &gt; x ^ 2 and x ^ 2 &gt; 81 }
            /// True
            /// ----------------------
            /// { 1, 2, 3 } /\ { 2, 3, 4 }
            /// { 2, 3 }
            /// ----------------------
            /// { 2, 3, 4 } /\ [-6; 2]
            /// { 2 }
            /// ----------------------
            /// [-6; 2] /\ [-3; 11]
            /// [-3; 2]
            /// [-6; 2] \/ [-3; 11]
            /// [-6; 11]
            /// [-6; 2] \ [-3; 11]
            /// [-6; -3)
            /// ----------------------
            /// { 1, 2, 3 } /\ { 2, 3, 4 }
            /// { 2, 3 }
            /// ----------------------
            /// 5 in [1; +oo) \/ { x : x &lt; -4 }
            /// True
            /// ----------------------
            /// { pi, e, 6, 11/2, 1 + 3i } /\ QQ
            /// { 6, 11/2 }
            /// { pi, e, 6, 11/2, 1 + 3i } /\ RR
            /// { pi, e, 6, 11/2 }
            /// { pi, e, 6, 11/2, 1 + 3i } /\ CC
            /// { pi, e, 6, 11/2, 1 + 3i }
            /// </code>
            /// </example>
            public sealed partial record ConditionalSet(Entity Var, Entity Predicate) : Set, IEquatable<ConditionalSet?>
            {
                /// <inheritdoc/>
                public override Entity Replace(Func<Entity, Entity> func)
                    => func(New(Var, Predicate.Replace(func)));

                /// <inheritdoc/>
                public override bool TryContains(Entity entity, out bool contains)
                {
                    contains = false;
                    var substituted = Predicate.Substitute(Var, entity);
                    substituted = substituted.Simplify();
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

                // The predicate is the child, but the set variable X is not the same as X out of the set,
                // so to avoid ambiguity, we replace it with a random variable name
                /// <inheritdoc/>
                protected override Entity[] InitDirectChildren() => new[] { Predicate.Substitute(Var, Variable.CreateRandom(Predicate)) };

                /// <inheritdoc/>
                public override bool IsSetFinite => false;
                /// <inheritdoc/>
                public override bool IsSetEmpty => Predicate.Evaled == Boolean.False;

                /// <summary>
                /// Compares two ConditionalSets
                /// If one is not CSet, false is returned
                /// </summary>
                public bool Equals(ConditionalSet? other)
                {
                    if (other is null) // invalid cast
                        return false;
                    var (one, two) = SetOperators.MergeToOneVariable(this, other);
                    return one.Predicate == two.Predicate;
                }


                [ConstantField] private readonly static Variable universalVoidConstant = Variable.CreateVariableUnchecked("%");
                /// <inheritdoc/>
                public override int GetHashCode()
                    => Predicate.Substitute(Var, universalVoidConstant).GetHashCode();

                /// <inheritdoc/>
                public override Set Filter(Entity predicate, Variable over) => new ConditionalSet(Var, Predicate & predicate.Substitute(over, Var));
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
                private static Dictionary<Domain, SpecialSet> InnerStorage => innerStorage ??= new();
                [ThreadStatic] private static Dictionary<Domain, SpecialSet>? innerStorage;

                /// <summary>
                /// Creates an instance of special set from a domain
                /// </summary>
                public static SpecialSet Create(Domain domain)
                { 
                    if (InnerStorage.TryGetValue(domain, out var res))
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
                    InnerStorage[domain] = result;
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
                    contains = MayContain(entity.Evaled);
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
                        => entity is Boolean || !entity.IsConstantLeaf;
                    internal override Domain ToDomain() => Domain.Boolean;
                }

                /// <summary>
                /// A set of all integers
                /// </summary>
                public sealed partial record Integers : SpecialSet
                {
                    /// <inheritdoc/>
                    public override bool MayContain(Entity entity)
                        => entity is Integer || !entity.IsConstantLeaf;
                    internal override Domain ToDomain() => Domain.Integer;
                }

                /// <summary>
                /// A set of all rational numbers
                /// </summary>
                public sealed partial record Rationals : SpecialSet
                {
                    /// <inheritdoc/>
                    public override bool MayContain(Entity entity)
                        => entity is Rational || !entity.IsConstantLeaf;
                    internal override Domain ToDomain() => Domain.Rational;
                }

                /// <summary>
                /// A set of all real numbers
                /// </summary>
                public sealed partial record Reals : SpecialSet
                {
                    /// <inheritdoc/>
                    public override bool MayContain(Entity entity)
                        => entity is Real || !entity.IsConstantLeaf;
                    internal override Domain ToDomain() => Domain.Real;
                }

                /// <summary>
                /// A set of all complex numbers
                /// </summary>
                public sealed partial record Complexes : SpecialSet
                {
                    /// <inheritdoc/>
                    public override bool MayContain(Entity entity)
                        => entity is Complex || !entity.IsConstantLeaf;
                    internal override Domain ToDomain() => Domain.Complex;
                }

                /// <inheritdoc/>
                public override Set Filter(Entity predicate, Variable over) => new ConditionalSet(over, predicate & over.In(this));
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
                public override bool IsSetFinite => isSetFinite.GetValue(static @this =>
                    @this.Left is FiniteSet finite1 && @this.Right is FiniteSet finite2 && finite1.IsSetFinite && finite2.IsSetFinite, this);
                private LazyPropertyA<bool> isSetFinite;

                /// <inheritdoc/>
                public override bool IsSetEmpty => isSetEmpty.GetValue(static @this =>
                    @this.Left is FiniteSet finite1 && @this.Right is FiniteSet finite2 && finite1.IsSetEmpty && finite2.IsSetEmpty, this);
                private LazyPropertyA<bool> isSetEmpty;

                /// <inheritdoc/>
                public override Set Filter(Entity predicate, Variable over)
                    => MathS.Union(
                        Left is Set setLeft ? setLeft.Filter(predicate, over) : Left,
                        Right is Set setRight ? setRight.Filter(predicate, over) : Right
                        );
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
                public override bool IsSetFinite => isSetFinite.GetValue(static @this => 
                    @this.Left is FiniteSet finite1 && @this.Right is FiniteSet finite2
                    && (finite1.IsSetFinite || finite2.IsSetFinite), this);
                private LazyPropertyA<bool> isSetFinite;

                /// <inheritdoc/>
                public override bool IsSetEmpty => isSetEmpty.GetValue(static @this =>
                    @this.Left is FiniteSet finite1 && @this.Right is FiniteSet finite2
                    && (finite1.IsSetEmpty || finite2.IsSetEmpty), this);
                private LazyPropertyA<bool> isSetEmpty;

                /// <inheritdoc/>
                public override Set Filter(Entity predicate, Variable over)
                    => MathS.Intersection(
                        Left is Set setLeft ? setLeft.Filter(predicate, over) : Left,
                        Right is Set setRight ? setRight.Filter(predicate, over) : Right
                        );
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
                public override bool IsSetFinite => isSetFinite.GetValue(static @this =>
                    @this.Left is FiniteSet finite1 && @this.Right is FiniteSet && finite1.IsSetFinite, this);
                private LazyPropertyA<bool> isSetFinite;

                /// <inheritdoc/>
                public override bool IsSetEmpty => isSetEmpty.GetValue(static @this =>
                    @this.Left is FiniteSet finite1 && @this.Right is FiniteSet finite2
                    && (finite1.IsSetEmpty || finite1 == finite2), this);
                private LazyPropertyA<bool> isSetEmpty;

                /// <inheritdoc/>
                public override Set Filter(Entity predicate, Variable over)
                    => MathS.SetSubtraction(
                        Left is Set setLeft ? setLeft.Filter(predicate, over) : Left,
                        Right is Set setRight ? setRight.Filter(predicate, over) : Right
                        );
            }
            #endregion
        }
#pragma warning restore CS1591 // TODO: it's only for records' parameters! Remove it once you can document records parameters

        /// <summary>
        /// Application of arguments to the given expression
        /// </summary>
        public sealed partial record Application(Entity Expression, LList<Entity> Arguments) : Entity
        {
            private Application New(Entity expr, LList<Entity> arguments)
                    => ReferenceEquals(Expression, expr)
                    && arguments.Count() == Arguments.Count()
                    && ((arguments, Arguments).SequencesAreEqualReferences())
                    ? this 
                    : new Application(expr, arguments);

            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func)
                    => func(New(Expression.Replace(func), Arguments.Map(a => a.Replace(func))));

            internal override Priority Priority => Priority.Func;

            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => (Expression + Arguments).ToArray();
        }

        /// <summary>
        /// A lambda with the only parameter and its body
        /// </summary>
        public sealed partial record Lambda(Variable Parameter, Entity Body) : Entity
        {
            private Lambda New(Variable parameter, Entity body)
                => ReferenceEquals(Parameter, parameter) && ReferenceEquals(Body, body)
                    ? this
                    : new Lambda(parameter, body);

            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func)
                => func(New(Parameter, Body.Replace(func)));

            internal override Priority Priority => Priority.Lambda;

            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new [] { Parameter, Body };
        }
    }
}
