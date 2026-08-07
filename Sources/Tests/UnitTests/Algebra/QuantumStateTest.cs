//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Algebra
{
    /// <summary>
    /// Symbolic quantum states. A state is an ordinary expression -- a ket is
    /// <c>apply(ket, 0, 1)</c> -- so most of the state algebra is the existing simplifier and
    /// is tested here to record that it is, rather than because new code does it.
    ///
    /// Everything is compared symbolically. No amplitude is evaluated to a double anywhere in
    /// this file, which is what makes <c>1/sqrt(2)</c> an exact answer rather than 0.7071.
    /// </summary>
    [Trait("Area", "Algebra")]
    public sealed class QuantumStateTest
    {
        private static Entity Ket(params int[] qubits) => MathS.Quantum.Ket(qubits);

        private static readonly Entity Sqrt2 = MathS.Sqrt(2);

        /// <summary>(|00&gt; + |11&gt;)/sqrt(2) -- entangled, and the canonical example of it.</summary>
        private static Entity Bell => (Ket(0, 0) + Ket(1, 1)) / Sqrt2;

        /// <summary>(|00&gt; + |01&gt;)/sqrt(2) -- a product state: the first qubit is |0&gt;.</summary>
        private static Entity Product => (Ket(0, 0) + Ket(0, 1)) / Sqrt2;

        /// <summary>(|000&gt; + |111&gt;)/sqrt(2).</summary>
        private static Entity Ghz => (Ket(0, 0, 0) + Ket(1, 1, 1)) / Sqrt2;

        private static void AssertSame(Entity expected, Entity actual) =>
            Assert.Equal(0, (expected - actual).Simplify().InnerSimplified);

        // ---- what the existing machinery already does, recorded rather than implemented ----

        [Fact]
        public void LikeTermsCollectWithoutAnyQuantumCode() =>
            AssertSame(("a + b").ToEntity() * Ket(0, 1),
                       ("a".ToEntity() * Ket(0, 1) + "b".ToEntity() * Ket(0, 1)).Simplify());

        [Fact]
        public void AZeroAmplitudeIsNotATerm() =>
            AssertSame(0, (0 * Ket(0, 1)).Simplify());

        [Fact]
        public void OppositeAmplitudesCancel() =>
            AssertSame(0, (Ket(0, 1) - Ket(0, 1)).Simplify());

        // ---- normalisation ----

        [Theory]
        [InlineData(true)]
        public void TheStandardStatesAreNormalised(bool _)
        {
            Assert.True(MathS.Quantum.IsNormalised(Bell));
            Assert.True(MathS.Quantum.IsNormalised(Product));
            Assert.True(MathS.Quantum.IsNormalised(Ghz));
            Assert.True(MathS.Quantum.IsNormalised(Ket(0, 1)));
        }

        /// <summary>
        /// The same state written with its amplitude in front rather than as a divisor. A
        /// reader that only looked to the left of a product would refuse this one, and a user
        /// would hit that on their first Bell state written the other way round.
        /// </summary>
        [Fact]
        public void AnAmplitudeInFrontIsReadTheSameAsADivisor()
        {
            Assert.True(MathS.Quantum.IsNormalised(1 / Sqrt2 * (Ket(0, 0) + Ket(1, 1))));
            AssertSame(Ket(0) * (Ket(0) + Ket(1)) * 2,
                       MathS.Quantum.Factorise(2 * (Ket(0, 0) + Ket(0, 1))));
        }

        [Fact]
        public void AnUnnormalisedStateIsNotNormalised()
        {
            Assert.False(MathS.Quantum.IsNormalised(Ket(0, 0) + Ket(1, 1)));
            Assert.False(MathS.Quantum.IsNormalised(2 * Ket(0)));
        }

        [Fact]
        public void SomethingThatIsNotAStateHasNoAnswer()
        {
            Assert.Null(MathS.Quantum.IsNormalised("x + 1".ToEntity()));
            Assert.Null(MathS.Quantum.IsNormalised("apply(ket, 5)".ToEntity()));
        }

        // ---- factorisation ----

        /// <summary>
        /// The first and last qubits agree across the superposition, so they factor out and
        /// only the middle carries it. This is the spine's <c>FactorOutCommon</c> -- the same
        /// call that takes the common monomial out of a polynomial.
        /// </summary>
        [Fact]
        public void ADefiniteQubitAtEitherEndFactorsOut()
        {
            var factored = MathS.Quantum.Factorise(Ket(0, 0, 1) + Ket(0, 1, 1));
            AssertSame(Ket(0) * (Ket(0) + Ket(1)) * Ket(1), factored);
        }

        [Fact]
        public void ADefiniteLeadingQubitFactorsOutAlone() =>
            AssertSame(Ket(0) * (Ket(0) + Ket(1)) / Sqrt2, MathS.Quantum.Factorise(Product));

        /// <summary>
        /// **Entanglement is the absence of this.** Neither qubit of a Bell state has a
        /// definite value, so nothing factors and the state comes back as it went in. Same for
        /// GHZ. A factoriser that "succeeded" on these would be wrong about physics.
        /// </summary>
        [Fact]
        public void AnEntangledStateDoesNotFactor()
        {
            AssertSame(Bell, MathS.Quantum.Factorise(Bell));
            AssertSame(Ghz, MathS.Quantum.Factorise(Ghz));
        }

        /// <summary>
        /// A known gap, pinned so it is not mistaken for a bug later:
        /// <c>(|0&gt;+|1&gt;)(|0&gt;+|1&gt;)</c> is separable, but no qubit is in a definite
        /// state, so the meet of the support is empty and nothing is factored. Detecting this
        /// is a rank-one test across a bipartition, which is not written yet.
        /// </summary>
        [Fact]
        public void SeparabilityWithoutADefiniteQubitIsNotYetDetected()
        {
            var uniform = Ket(0, 0) + Ket(0, 1) + Ket(1, 0) + Ket(1, 1);
            AssertSame(uniform, MathS.Quantum.Factorise(uniform));
        }

        // ---- round trip ----

        /// <summary>
        /// Expanding a factorisation returns the state it came from. This is the strongest
        /// check here: the two directions are written independently -- factoring takes the meet
        /// of the support, expanding concatenates widths -- so agreement is evidence rather
        /// than tautology.
        /// </summary>
        [Theory]
        [InlineData("(0,0,1)+(0,1,1)")]
        [InlineData("(0,0)+(0,1)")]
        [InlineData("(1,0,0)+(1,0,1)")]
        public void ExpandingAFactorisationReturnsTheOriginal(string which)
        {
            var original = which switch
            {
                "(0,0,1)+(0,1,1)" => Ket(0, 0, 1) + Ket(0, 1, 1),
                "(0,0)+(0,1)" => Ket(0, 0) + Ket(0, 1),
                _ => Ket(1, 0, 0) + Ket(1, 0, 1),
            };
            var roundTripped = MathS.Quantum.TensorExpand(MathS.Quantum.Factorise(original));
            AssertSame(original, roundTripped);
        }

        [Fact]
        public void ExpandingAProductOfStatesMultipliesItOut() =>
            AssertSame(Ket(0, 0) + Ket(0, 1) + Ket(1, 0) + Ket(1, 1),
                       MathS.Quantum.TensorExpand((Ket(0) + Ket(1)) * (Ket(0) + Ket(1))));

        // ---- equality up to a global phase ----

        /// <summary>
        /// A global factor is exactly the difference no measurement can see, so these are the
        /// same state written two ways.
        /// </summary>
        [Fact]
        public void AGlobalSignIsNotADifference() =>
            Assert.True(MathS.Quantum.EqualUpToGlobalPhase(
                Ket(0, 0) + Ket(1, 1), -Ket(0, 0) - Ket(1, 1)));

        [Fact]
        public void AGlobalImaginaryPhaseIsNotADifference() =>
            Assert.True(MathS.Quantum.EqualUpToGlobalPhase(
                Ket(0, 0) + Ket(1, 1),
                MathS.i * Ket(0, 0) + MathS.i * Ket(1, 1)));

        /// <summary>
        /// A *relative* phase is a real difference: |00&gt; - |11&gt; is not the Bell state,
        /// and this is the case a naive ratio check would get wrong.
        /// </summary>
        [Fact]
        public void ARelativePhaseIsADifference() =>
            Assert.False(MathS.Quantum.EqualUpToGlobalPhase(
                Ket(0, 0) + Ket(1, 1), Ket(0, 0) - Ket(1, 1)));

        /// <summary>
        /// Rescaling by something that is not a unit changes the state's norm, so it is not
        /// the same state seen differently.
        /// </summary>
        [Fact]
        public void ScalingByANonUnitIsADifference() =>
            Assert.False(MathS.Quantum.EqualUpToGlobalPhase(
                Ket(0, 0) + Ket(1, 1), 2 * Ket(0, 0) + 2 * Ket(1, 1)));

        [Fact]
        public void DifferentSupportsAreDifferentStates() =>
            Assert.False(MathS.Quantum.EqualUpToGlobalPhase(
                Ket(0, 0) + Ket(1, 1), Ket(0, 0) + Ket(0, 1)));
    }
}
