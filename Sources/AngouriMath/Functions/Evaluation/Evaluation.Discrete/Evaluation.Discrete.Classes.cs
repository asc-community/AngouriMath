//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using PeterO.Numbers;
using AngouriMath.Core.Sets;
using AngouriMath.Functions;
using static AngouriMath.Entity.Boolean;
using static AngouriMath.Entity.Set;

namespace AngouriMath
{
    partial record Entity
    {
        partial record Boolean
        {
            // Boolean values are always defined
            private protected override Entity IntrinsicCondition => True;
            
            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact) => this;
        }

        partial record Notf
        {
            // Logical NOT is always defined for any input
            private protected override Entity IntrinsicCondition => True;

            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact)
                => ExpandOnOneArgument(Argument,
                    a => a switch
                    {
                        Boolean(var b) => !b,
                        // A negation passes through a quantifier by flipping it: not every
                        // member satisfies P exactly when some member fails it, and the set
                        // never changes. Inward, since the flipped statement is one the
                        // quantifier may decide where the negation of an undecided one is not.
                        // https://github.com/asc-community/AngouriMath/issues/1409
                        Forallf(var v, var over, var body) => new Existsf(v, over, !body).InnerSimplified(isExact),
                        Existsf(var v, var over, var body) => new Forallf(v, over, !body).InnerSimplified(isExact),
                        _ => null
                    },
                    (@this, a) => ((Notf)@this).New(a), isExact);
        }

        /// <summary>
        /// Whether either operand of a logical connective is a number, which is not a truth value:
        /// <c>0</c> is not <see langword="false"/> and <c>1</c> is not <see langword="true"/>. A
        /// connective declines such a pair rather than answering, and declines it even where its
        /// table could answer without looking -- <c>false and 0</c> is not a proposition to be
        /// false about.
        /// </summary>
        /// <remarks>
        /// <c>NaN</c> is excluded deliberately, and the distinction is the point. <c>NaN</c> is how
        /// this library spells <em>no truth value</em>, which is what an order comparison over the
        /// complex plane produces, and what a connective does with one is settled by the
        /// three-valued table: <c>false and NaN</c> is <see langword="false"/>
        /// (https://github.com/asc-community/AngouriMath/issues/880). A number is the other thing
        /// entirely -- a value of the wrong sort, where the question rather than the answer is at
        /// fault. https://github.com/asc-community/AngouriMath/issues/897
        /// </remarks>
        private static bool MixesANumberWithATruthValue(Entity left, Entity right)
            => IsNotATruthValue(left) || IsNotATruthValue(right);

        private static bool IsNotATruthValue(Entity operand)
            => operand.Evaled is Number && !operand.Evaled.IsNaN;

        partial record Andf
        {
            // Logical AND is always defined for any inputs
            private protected override Entity IntrinsicCondition => True;
            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact)
                => ExpandOnTwoArguments(Left, Right,
                    (left, right) => MixesANumberWithATruthValue(left, right) ? null
                        : left == right ? left
                        : (left.Evaled, right.Evaled) switch
                    {
                        (Boolean(false), _) or (_, Boolean(false)) => False,
                        (Boolean(true), _) => right,
                        (_, Boolean(true)) => left,
                        _ => WithoutRepeatedOrContradictoryConjuncts(left, right)
                    },
                    (@this, a, b) => ((Andf)@this).New(a, b), isExact, settlesNaN: true);

            // A predicate joined to a predicate repeats what the two share and pairs the three
            // signs of one quantity -- `q = 0 and q > 0` -- that no value satisfies. A piecewise
            // combined with a piecewise joins every case's predicate to every other's, so with
            // the repeats and the contradictions left as written the cases multiply instead of
            // staying three: 3, 9, 27, ... 3^12 of them and 8 GB inside integration by parts on
            // (c - c/(a^2 x^2))^(9/2) / e^(3 acoth(a x)). A conjunct already on the left is not
            // written again, and one that contradicts a conjunct on the left makes the whole
            // conjunction False, which is what lets the piecewise drop the case.
            // https://github.com/asc-community/AngouriMath/issues/1414
            private static Entity? WithoutRepeatedOrContradictoryConjuncts(Entity left, Entity right)
                => Survey(left, right) switch
                {
                    Verdict.Contradiction => False,
                    Verdict.Repeats => Rebuilt(left, left, right),
                    _ => null
                };

            private enum Verdict { Unchanged, Repeats, Contradiction }

            // Walks the conjuncts of `right` against those of `left` without building a list:
            // this runs on every conjunction the evaluator meets, most of which have nothing to
            // drop, and a list per call was 8.7% of SimplifyHard's allocation. Both operands are
            // already simplified, so a repeat or a contradiction inside `right` alone is gone
            // before it arrives here; only pairs across the two sides are asked about.
            private static Verdict Survey(Entity left, Entity right)
            {
                if (right is Andf(var first, var second))
                {
                    var one = Survey(left, first);
                    if (one == Verdict.Contradiction)
                        return one;
                    var other = Survey(left, second);
                    if (other == Verdict.Contradiction)
                        return other;
                    return one == Verdict.Repeats || other == Verdict.Repeats ? Verdict.Repeats : Verdict.Unchanged;
                }
                if (IsAmong(left, right))
                    return Verdict.Repeats;
                if (ContradictsAny(left, right))
                    return Verdict.Contradiction;
                return Verdict.Unchanged;
            }

            // `left` with the conjuncts of `right` that are not already in it, folded on the
            // left as the parser folds a chain.
            private static Entity Rebuilt(Entity conjunction, Entity left, Entity right)
                => right is Andf(var first, var second)
                    ? Rebuilt(Rebuilt(conjunction, left, first), left, second)
                    : IsAmong(left, right) ? conjunction : new Andf(conjunction, right);

            private static bool IsAmong(Entity conjunction, Entity atom)
                => conjunction is Andf(var first, var second)
                    ? IsAmong(first, atom) || IsAmong(second, atom)
                    : conjunction == atom;

            private static bool ContradictsAny(Entity conjunction, Entity atom)
                => conjunction is Andf(var first, var second)
                    ? ContradictsAny(first, atom) || ContradictsAny(second, atom)
                    : Contradict(conjunction, atom);

            // The two contradict when no value satisfies both. Only pairs with a truth value
            // everywhere are decided here: an equality is True or False whatever its operands,
            // while an order comparison is NaN off the real line, so `x < 0 and x >= 0` is NaN
            // at x = i and reducing it to False is Simplify's business, under `x in RR`
            // (https://github.com/asc-community/AngouriMath/issues/876). `q = 0 and q > 0` is
            // False everywhere: where q is not real the equality is False, and False and
            // anything is False (https://github.com/asc-community/AngouriMath/issues/880).
            private static bool Contradict(Entity one, Entity other)
                => (one is Notf(var negated) && negated == other && HasATruthValueEverywhere(other))
                || (other is Notf(var negatedOther) && negatedOther == one && HasATruthValueEverywhere(one))
                || (one is ComparisonSign and IBinaryNode { NodeFirstChild: var a, NodeSecondChild: var b } && other is ComparisonSign and IBinaryNode { NodeFirstChild: var c, NodeSecondChild: var d }
                    && a == c && b == d
                    && (one, other) is (Equalsf, Greaterf) or (Greaterf, Equalsf)
                        or (Equalsf, Lessf) or (Lessf, Equalsf));

            private static bool HasATruthValueEverywhere(Entity statement)
                => statement is Equalsf;
        }

        partial record Orf
        {
            // Logical OR is always defined for any inputs
            private protected override Entity IntrinsicCondition => True;
            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact)
                => ExpandOnTwoArguments(Left, Right,
                    (left, right) => MixesANumberWithATruthValue(left, right) ? null
                        : (left.Evaled, right.Evaled) switch
                    {
                        (Boolean(true), _) or (_, Boolean(true)) => True,
                        (Boolean(false), _) => right,
                        (_, Boolean(false)) => left,
                        _ => null
                    },
                    (@this, a, b) => ((Orf)@this).New(a, b), isExact, settlesNaN: true);
        }

        partial record Xorf
        {
            // Logical XOR is always defined for any inputs
            private protected override Entity IntrinsicCondition => True;
            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact)
                => ExpandOnTwoArguments(Left, Right,
                    (left, right) => MixesANumberWithATruthValue(left, right) ? null
                        : (left.Evaled, right.Evaled) switch
                    {
                        (Boolean(var leftBool), Boolean(var rightBool)) => leftBool ^ rightBool,
                        (Boolean(true), _) => !right,
                        (Boolean(false), _) => right,
                        (_, Boolean(true)) => !left,
                        (_, Boolean(false)) => left,
                        _ => null
                    },
                    (@this, a, b) => ((Xorf)@this).New(a, b), isExact, settlesNaN: true);
        }

        partial record Impliesf
        {
            // Logical implication is always defined for any inputs
            private protected override Entity IntrinsicCondition => True;
            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact)
                => ExpandOnTwoArguments(Assumption, Conclusion,
                    (left, right) => MixesANumberWithATruthValue(left, right) ? null
                        : (left.Evaled, right.Evaled) switch
                    {
                        (Boolean(var leftBool), Boolean(var rightBool)) => !leftBool || rightBool,
                        (Boolean(false), _) => True,
                        (Boolean(true), _) => right,
                        (_, Boolean(true)) => True,
                        (_, Boolean(false)) => !left,
                        _ => null
                    },
                    (@this, a, b) => ((Impliesf)@this).New(a, b), isExact, settlesNaN: true);
        }

        partial record Equalsf
        {
            // Equality comparison is always defined for any inputs
            private protected override Entity IntrinsicCondition => True;

            /// <summary>
            /// Decides equality of two constants.
            /// </summary>
            /// <remarks>
            /// Comparing the separately evaluated values for exact digit equality is not
            /// enough. sqrt(i) and (1 + i) / sqrt(2) are the same number, but evaluating
            /// each of them rounds independently, so the results disagree in the last few
            /// digits and the comparison used to answer False. Their difference, on the
            /// other hand, cancels, and <see cref="Number.Real"/>'s factory maps anything
            /// below <see cref="MathS.Settings.PrecisionErrorZeroRange"/> onto an exact
            /// zero -- so testing the difference is both more robust and consistent with
            /// how the rest of the library already decides what counts as zero.
            /// </remarks>
            private static bool ConstantsAreEqual(Entity left, Entity right)
                => left.Evaled == right.Evaled
                    || (left - right).Evaled is Number.Complex difference
                        && Number.IsZero(difference);

            /// <summary>
            /// Two sets are equal exactly when each is a subset of the other -- the double
            /// containment that defines set equality -- so a pair of sets that are not the same
            /// node is put to <see cref="Set.Subsetf"/> both ways: <c>{ x in ZZ : x >= 1 } = ZZ+</c>
            /// is <c>True</c>, <c>[0; 1] = [0; 2]</c> is <c>False</c>, and a pair the subset
            /// decision leaves open stays written.
            /// https://github.com/asc-community/AngouriMath/issues/1409
            /// </summary>
            private static Entity? SetsAreEqual(Set left, Set right, bool isExact)
                => (SetOperators.Subset(left, right, isExact), SetOperators.Subset(right, left, isExact)) switch
                {
                    (Boolean a, Boolean b) when a == True && b == True => True,
                    (Boolean a, _) when a == False => False,
                    (_, Boolean b) when b == False => False,
                    _ => null,
                };

            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact)
                => ExpandOnTwoArguments(Left, Right,
                    (left, right) => left == right ? true
                    : left.IsConstant && right.IsConstant ? ConstantsAreEqual(left, right)
                    : left is Set setLeft && right is Set setRight ? SetsAreEqual(setLeft, setRight, isExact)
                    : null,
                    (@this, a, b) => ((Equalsf)@this).New(a, b), isExact,
                    // An equality of two sets is one statement about the two, and stays one when
                    // it is not decided: { 1, x } = { 1, 2 } was { True, False, x = 1, x = 2 },
                    // the comparison distributed over the members of each side.
                    propagateSet: !(Left.InnerSimplified(isExact) is Set && Right.InnerSimplified(isExact) is Set));
        }

        partial record Greaterf
        {
            // Inequality comparisons are only defined for real numbers.
            // For non-real complex numbers, they evaluate to NaN.
            private protected override Entity IntrinsicCondition => 
                Left.In(MathS.Sets.R) & Right.In(MathS.Sets.R);
            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact)
                => ExpandOnTwoArguments(Left, Right,
                    (a, b) => (a, b) switch
                    {
                        (Real reLeft, Real reRight) => reLeft > reRight,
                        (Number numLeft, Number numRight) => MathS.NaN,
                        _ => null
                    },
                    (@this, a, b) => ((Greaterf)@this).New(a, b), isExact);
        }

        partial record GreaterOrEqualf
        {
            // Inequality comparisons are only defined for real numbers.
            // For non-real complex numbers, they evaluate to NaN.
            private protected override Entity IntrinsicCondition => 
                Left.In(MathS.Sets.R) & Right.In(MathS.Sets.R);

            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact)
                => ExpandOnTwoArguments(Left, Right,
                    (a, b) => (a, b) switch
                    {
                        (Real reLeft, Real reRight) => reLeft >= reRight,
                        (Number numLeft, Number numRight) => MathS.NaN,
                        _ => null
                    },
                    (@this, a, b) => ((GreaterOrEqualf)@this).New(a, b), isExact);
        }

        partial record Lessf
        {
            // Inequality comparisons are only defined for real numbers.
            // For non-real complex numbers, they evaluate to NaN.
            private protected override Entity IntrinsicCondition => 
                Left.In(MathS.Sets.R) & Right.In(MathS.Sets.R);

            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact)
                => ExpandOnTwoArguments(Left, Right,
                    (a, b) => (a, b) switch
                    {
                        (Real reLeft, Real reRight) => reLeft < reRight,
                        (Number numLeft, Number numRight) => MathS.NaN,
                        _ => null
                    },
                    (@this, a, b) => ((Lessf)@this).New(a, b), isExact);
        }

        partial record LessOrEqualf
        {
            // Inequality comparisons are only defined for real numbers.
            // For non-real complex numbers, they evaluate to NaN.
            private protected override Entity IntrinsicCondition => 
                Left.In(MathS.Sets.R) & Right.In(MathS.Sets.R);
            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact)
                => ExpandOnTwoArguments(Left, Right,
                    (a, b) => (a, b) switch
                    {
                        (Real reLeft, Real reRight) => reLeft <= reRight,
                        (Number numLeft, Number numRight) => MathS.NaN,
                        _ => null
                    },
                    (@this, a, b) => ((LessOrEqualf)@this).New(a, b), isExact);
        }

        partial record Set
        {
            partial record Inf
            {
                // Set membership is always defined for any element and set
                private protected override Entity IntrinsicCondition => True;
                /// <inheritdoc/>
                protected override Entity InnerSimplify(bool isExact)
                    => ExpandOnTwoArguments(Element, SupSet,
                        (a, b) => (a, b) switch
                        {
                            (var el, Set set) when set.TryContains(el, out var contains) => contains,
                            _ => null
                        },
                        (@this, a, b) => ((Inf)@this).New(a, b), isExact, propagateSet: false);
            }

            partial record Subsetf
            {
                // A statement about two sets: a number or a truth value on either side is not
                // one, the way an inequality is NaN off the real line.
                private protected override Entity IntrinsicCondition => True;
                /// <inheritdoc/>
                protected override Entity InnerSimplify(bool isExact)
                    => ExpandOnTwoArguments(Sub, Super,
                        (a, b) => (a, b) switch
                        {
                            (Number or Boolean, _) or (_, Number or Boolean) => MathS.NaN,
                            _ => SetOperators.Subset(a, b, isExact),
                        },
                        (@this, a, b) => ((Subsetf)@this).New(a, b), isExact, propagateSet: false);
            }
        }

        partial record Dividesf
        {
            // Divisibility is a statement about integers; over anything else it is NaN, the way
            // an inequality is over a non-real number.
            private protected override Entity IntrinsicCondition
                => Divisor.In(MathS.Sets.Z) & Dividend.In(MathS.Sets.Z);

            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact)
                => ExpandOnTwoArguments(Divisor, Dividend,
                    (a, b) => (a, b) switch
                    {
                        // 0 divides b exactly when b is 0: b = k * 0 has a solution only then.
                        (Integer divisor, Integer dividend) when divisor.EInteger.IsZero
                            => dividend.EInteger.IsZero ? True : False,
                        (Integer divisor, Integer dividend)
                            => dividend.EInteger.Remainder(divisor.EInteger).IsZero ? True : False,
                        (Number, Number) => MathS.NaN,
                        _ => null
                    },
                    (@this, a, b) => ((Dividesf)@this).New(a, b), isExact);
        }

        partial record Congruentf
        {
            // A congruence is a statement about integers, like divisibility; over anything else
            // it is NaN.
            private protected override Entity IntrinsicCondition
                => Left.In(MathS.Sets.Z) & Right.In(MathS.Sets.Z) & Modulus.In(MathS.Sets.Z);

            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact)
            {
                var left = Left.InnerSimplified(isExact);
                var right = Right.InnerSimplified(isExact);
                var modulus = Modulus.InnerSimplified(isExact);
                if (left.IsNaN || right.IsNaN || modulus.IsNaN)
                    return MathS.NaN;
                return Decide(left, right, modulus) ?? New(left, right, modulus);
            }

            // Decided where n | a - b is: for numbers outright, and for symbols where a - b is a
            // polynomial every term of which the modulus divides -- (n - a)^2 = a^2 (mod n) has
            // the difference n^2 - 2 a n, and (x + y)^5 = x^5 + y^5 (mod 5) a difference whose
            // every coefficient is a multiple of 5. Where the coefficients are not all multiples
            // the congruence may still hold for particular values, so nothing is said.
            private static Entity? Decide(Entity left, Entity right, Entity modulus)
            {
                switch (left, right, modulus)
                {
                    case (Integer a, Integer b, Integer n):
                        return n.EInteger.IsZero
                            ? a.EInteger.Equals(b.EInteger) ? Boolean.True : Boolean.False
                            : a.EInteger.Subtract(b.EInteger).Remainder(n.EInteger).IsZero ? Boolean.True : Boolean.False;
                    case (Number, Number, Number):
                        return MathS.NaN;
                }
                var difference = (left - right).InnerSimplified;
                if (difference is Integer whole && modulus is Integer m)
                    return m.EInteger.IsZero
                        ? whole.EInteger.IsZero ? Boolean.True : Boolean.False
                        : whole.EInteger.Remainder(m.EInteger).IsZero ? Boolean.True : Boolean.False;
                if (difference.Complexity > LargestDifferenceRead)
                    return null;
                var variables = difference.Vars.Concat(modulus.Vars).Distinct()
                    .OrderBy(v => v.Name, System.StringComparer.Ordinal).ToArray();
                if (variables.Length == 0 || variables.Length > MultivariatePolynomial.MaxVariables)
                    return null;
                var indices = new Dictionary<Variable, int>();
                for (var i = 0; i < variables.Length; i++)
                    indices[variables[i]] = i;
                if (MultivariatePolynomial.TryParse(difference, indices) is not { } polynomial)
                    return null;
                if (polynomial.IsZero)
                    return Boolean.True;
                if (modulus is Integer integerModulus && !integerModulus.EInteger.IsZero)
                    return polynomial.Terms.All(term => term.Value.IsInteger() && term.Value.Numerator.Remainder(integerModulus.EInteger).IsZero)
                        ? Boolean.True : null;
                if (MultivariatePolynomial.TryParse(modulus, indices) is { IsZero: false, IsConstant: false } divisor)
                    return polynomial.DivideExact(divisor, out _) is not null ? Boolean.True : null;
                return null;
            }

            // The polynomial route expands the difference, which is worth doing for the sizes a
            // congruence is written at and not for an expression that arrives as a byproduct.
            private const int LargestDifferenceRead = 2048;
        }

        partial record Quantifier
        {
            // A quantified statement has a truth value wherever its body does at every member;
            // it is decided or left as written, never NaN on its own account.
            private protected override Entity IntrinsicCondition => True;

            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact)
            {
                var over = Over.InnerSimplified(isExact);
                var body = Body.InnerSimplified(isExact);
                return Functions.Boolean.Quantifiers.Decide(Kind, Var, over, body, isExact) ?? New(Var, over, body);
            }

            private Functions.Boolean.Quantifiers.Kind Kind => this switch
            {
                Forallf => Functions.Boolean.Quantifiers.Kind.All,
                Existsf => Functions.Boolean.Quantifiers.Kind.Some,
                _ => Functions.Boolean.Quantifiers.Kind.Unique,
            };
        }

        partial record Cardf
        {
            // A cardinality is defined for every set; what it is not is always a number here.
            private protected override Entity IntrinsicCondition => True;

            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact)
                => ExpandOnOneArgument(Argument,
                    a => a switch
                    {
                        // {x, 1} has two elements unless x is 1, so a set is counted once its
                        // elements are numbers, which are distinct exactly when unequal -- or
                        // listed sets of such, which are compared by their members: { 1, {} }
                        // has two members and { {} } has one (the reference's §3.3.7).
                        FiniteSet finite when finite.All(Countable)
                            => Integer.Create(finite.Count),
                        // A power set of a counted set has 2^n members.
                        Powersetf(var of) when MathS.Sets.Card(of).InnerSimplified(isExact) is Integer { EInteger: var counted }
                            && counted.Sign >= 0 && counted.CanFitInInt32()
                            => Integer.Create(EInteger.One.ShiftLeft(counted.ToInt32Checked())),
                        // An interval with numeric ends: one point, or none, is countable; a
                        // proper interval is not, and there is no number for it here.
                        Interval { Left: Real left, Right: Real right } interval => left.EDecimal.CompareTo(right.EDecimal) switch
                        {
                            > 0 => Integer.Zero,
                            0 => interval.LeftClosed && interval.RightClosed ? Integer.One : Integer.Zero,
                            _ => null
                        },
                        _ => null
                    },
                    // Not propagated into the set: card({1, 2}) is a count of the set, not a
                    // set of counts.
                    (@this, a) => ((Cardf)@this).New(a), isExact, propagateSet: false);

            private static bool Countable(Entity element)
                => element is Number || element is FiniteSet listed && listed.All(Countable);
        }

        partial record Phif
        {
            // Euler's totient function is defined for all integers in this library.
            // For positive integers, it returns the standard φ(n) value.
            // For non-positive integers, this implementation extends the definition by returning 0.
            private protected override Entity IntrinsicCondition => Argument.In(MathS.Sets.Z);

            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact)
                => ExpandOnOneArgument(Argument,
                    a => a switch
                    {
                        Integer integer => integer.Phi(),
                        Number n => MathS.NaN,
                        _ => null
                    },
                    (@this, a) => ((Phif)@this).New(a), isExact);
        }
    }
}
