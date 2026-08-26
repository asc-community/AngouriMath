//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Collections.Generic;
using System.Linq;
using AngouriMath;
using AngouriMath.Extensions;
using AngouriMath.Functions;
using Xunit;
using static AngouriMath.Entity;

namespace AngouriMath.Tests.Algebra.Polynomials
{
    /// <summary>
    /// <see cref="EvaluationHomomorphism.CertifiesIrreducible"/> — a proof that a polynomial in
    /// several variables does not factor, from an image in one of them.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The half that must never be wrong is the <see langword="true"/> half, since nothing
    /// downstream can check it: a factorisation is verified by division, and "there is no
    /// factorisation" is not. So the reducible corpus below is the real subject of this file,
    /// and it is built as <b>products</b> — every case is reducible by construction, whatever
    /// the factoriser would say about it.
    /// </para>
    /// <para>
    /// The <see langword="false"/> half claims nothing, so the irreducible corpus is a
    /// measurement of reach rather than an assertion about correctness. It is written as a
    /// count so that losing reach fails rather than passing quietly.
    /// </para>
        /// <para>
        /// <b>The negative half is not vacuous, and <see cref="TheIrreducibleOnesAreCertified"/>
        /// is what says so.</b> A certificate that always declined would pass every
        /// <c>AssertFalse</c> here; it is the count in that test which requires the machinery to
        /// actually reach a verdict, so the two halves hold each other up.
        /// </para>
    /// </remarks>
    [Trait("Area", "Algebra")]
    public sealed class EvaluationIrreducibilityTest
    {
        private static IReadOnlyDictionary<Variable, int> Index(params string[] names)
        {
            var index = new Dictionary<Variable, int>();
            for (var i = 0; i < names.Length; i++)
                index[(Variable)names[i]] = i;
            return index;
        }

        private static bool Certifies(string source, string main, params string[] variables)
        {
            var parsed = MultivariatePolynomial.TryParse(source.ToEntity(), Index(variables));
            Assert.True(parsed is not null, $"{source} was not read as a polynomial");
            return EvaluationHomomorphism.CertifiesIrreducible(parsed!, System.Array.IndexOf(variables, main));
        }

        /// <summary>
        /// Every one of these is a product, so a certificate on any of them is a wrong answer.
        /// </summary>
        [Theory]
        [InlineData("(x + y) * (x - y)")]
        [InlineData("(x + y) * (x + 2 * y)")]
        [InlineData("(x + y + 1) * (x - y + 1)")]
        [InlineData("(x ^ 2 + y) * (x ^ 2 - y)")]
        [InlineData("(x ^ 2 + y ^ 5) * (x ^ 2 - y ^ 5)")]
        [InlineData("(x ^ 8 + y) * (x ^ 8 + 3 * y)")]
        [InlineData("(x + y) * (x + y)")]
        [InlineData("(x - 1) * (x - y)")]
        [InlineData("(x + 2 * y) * (3 * x - y)")]
        [InlineData("(x ^ 3 + y) * (x + y ^ 3)")]
        [InlineData("(x + y) * (x - y) * (x + 2 * y)")]
        [InlineData("(x ^ 2 + x * y + 1) * (x ^ 2 - x * y + 1)")]
        public void AProductIsNeverCertified(string source)
            => Assert.False(Certifies(source, "x", "x", "y"),
                $"{source} is a product and was certified irreducible");

        /// <summary>
        /// The same, written out rather than as a product — these are the shapes
        /// <see cref="KroneckerFactorization"/>'s own remark names as ones it refuses, so a
        /// certificate here would be a wrong answer on exactly the cases this is meant to help.
        /// </summary>
        [Theory]
        [InlineData("x ^ 7 - y ^ 7")]
        [InlineData("x ^ 6 - y ^ 6")]
        [InlineData("x ^ 2 - y ^ 2")]
        [InlineData("x ^ 4 - y ^ 4")]
        [InlineData("x ^ 3 - y ^ 3")]
        public void ADifferenceOfLikePowersIsNeverCertified(string source)
            => Assert.False(Certifies(source, "x", "x", "y"),
                $"{source} has x - y as a factor and was certified irreducible");

        /// <summary>
        /// A factor free of the main variable is invisible to an image in it, which is what the
        /// content precondition is for. Without it <c>y * (x + 1)</c> is certified on an image
        /// of <c>2x + 2</c>.
        /// </summary>
        [Theory]
        [InlineData("y * (x + 1)")]
        [InlineData("y ^ 2 * (x ^ 2 + 1)")]
        [InlineData("(y + 1) * (x ^ 2 + x + 1)")]
        [InlineData("y * z * (x + 1)")]
        public void AContentInTheMainVariableDeclines(string source)
            => Assert.False(Certifies(source, "x", "x", "y", "z"),
                $"{source} has a factor free of x and was certified irreducible");

        /// <summary>
        /// What it reaches. Each of these is irreducible over the rationals, and the count is
        /// asserted so that losing reach fails rather than passing quietly.
        /// </summary>
        [Fact]
        public void TheIrreducibleOnesAreCertified()
        {
            var cases = new[]
            {
                ("x ^ 2 + y ^ 2 + 1", new[] { "x", "y" }),
                ("x ^ 2 + y", new[] { "x", "y" }),
                ("x ^ 3 + y + 1", new[] { "x", "y" }),
                ("x ^ 2 + x * y + y ^ 2 + 1", new[] { "x", "y" }),
                ("x ^ 2 + y ^ 2 + z ^ 2 + 1", new[] { "x", "y", "z" }),
                ("x ^ 2 + y ^ 2 + z ^ 2 + w ^ 2 + 1", new[] { "x", "y", "z", "w" }),
                ("x ^ 5 + y + 1", new[] { "x", "y" }),
                ("x ^ 4 + x + y", new[] { "x", "y" }),
            };
            var certified = cases.Count(one => Certifies(one.Item1, "x", one.Item2));
            Assert.True(certified >= 7,
                $"only {certified} of {cases.Length} irreducible polynomials were certified");
        }

        /// <summary>
        /// The four-variable case is the point of this: its Kronecker image has degree
        /// <c>3·3·3·3 - 1 = 80</c>, past the one-variable factoriser's 32, so the substitution
        /// refuses it outright. An evaluation image has degree 2 however many variables there
        /// are.
        /// </summary>
        [Fact]
        public void ItReachesPastTheSubstitutionsCeiling()
        {
            const string source = "x ^ 2 + y ^ 2 + z ^ 2 + w ^ 2 + 1";
            Assert.True(Certifies(source, "x", "x", "y", "z", "w"));
            // And the substitution cannot: 3·3·3·3 - 1 = 80 is past the one-variable
            // factoriser's 32. Before the certificate, `Factor` refused with null here; it hands
            // the polynomial back now, which is the difference this makes to a caller.
            var factored = MathS.Polynomials.Factor(source.ToEntity(), (Variable)"x");
            Assert.NotNull(factored);
            // Compared by value rather than by spelling, since the answer comes back through the
            // polynomial representation and its terms are in that order rather than the input's.
            Assert.Equal(Number.Integer.Zero, (factored! - source.ToEntity()).Simplify());
            // And it is the polynomial itself rather than a factorisation of it, which is the
            // whole content of the answer: there is nothing to find.
            Assert.IsNotType<Mulf>(factored);
        }
    }
}
