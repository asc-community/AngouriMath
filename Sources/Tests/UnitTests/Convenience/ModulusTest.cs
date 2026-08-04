//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Core.Exceptions;
using AngouriMath.Extensions;
using System;
using System.Numerics;
using Xunit;

namespace AngouriMath.Tests.Convenience
{
    /// <summary>
    /// The remainder after division, asked for by
    /// https://github.com/asc-community/AngouriMath/issues/402 and
    /// https://github.com/asc-community/AngouriMath/issues/618. There was no node for it at
    /// all: <c>%</c> was a parse error and <c>MathS.Mod</c> did not exist.
    /// </summary>
    public sealed class ModulusTest
    {
        private static readonly Entity.Variable x = MathS.Var("x");

        /// <summary>
        /// The remainder takes the sign of the dividend -- the truncated convention, which is
        /// what C# itself does and what the arbitrary-precision arithmetic underneath does.
        /// (-7) % 3 is -1 and not 2, and that has to be pinned, because the Euclidean
        /// convention is the other common choice and the two disagree on every negative
        /// dividend.
        /// </summary>
        [Theory]
        [InlineData("7 % 3", "1")]
        [InlineData("(-7) % 3", "-1")]
        [InlineData("7 % (-3)", "1")]
        [InlineData("(-7) % (-3)", "-1")]
        [InlineData("6 % 3", "0")]
        [InlineData("2 % 5", "2")]
        [InlineData("7.5 % 2", "3/2")]
        [InlineData("(1/2) % (1/3)", "1/6")]
        public void TheRemainderFollowsTheDividendsSign(string expression, string expected) =>
            Assert.Equal(expected.ToEntity().Evaled, expression.ToEntity().Evaled);

        /// <summary>Dividing by zero is undefined, as it is for the quotient.</summary>
        [Fact]
        public void ByZeroIsUndefined() =>
            Assert.Equal(MathS.NaN.Evaled, "7 % 0".ToEntity().Evaled);

        /// <summary>
        /// % binds as tightly as * and /, and associates to the left, which is what it does in
        /// C# and what anyone writing the expression will assume.
        /// </summary>
        [Theory]
        [InlineData("2 * 7 % 3", 2 * 7 % 3)]
        [InlineData("7 % 3 * 2", 7 % 3 * 2)]
        [InlineData("12 / 4 % 2", 12 / 4 % 2)]
        [InlineData("1 + 7 % 3", 1 + 7 % 3)]
        [InlineData("7 % 3 + 1", 7 % 3 + 1)]
        [InlineData("2 ^ 3 % 3", 8 % 3)]
        [InlineData("-7 % 3", -7 % 3)]
        public void ItBindsLikeItDoesInCSharp(string expression, int expected) =>
            Assert.Equal(Entity.Number.Integer.Create(expected), expression.ToEntity().Evaled);

        [Fact]
        public void TheOperatorBuildsTheSameNodeAsTheParser() =>
            Assert.Equal("x % 3".ToEntity(), x % 3);

        [Fact]
        public void MathSModBuildsTheSameNodeAsTheParser() =>
            Assert.Equal("x % 3".ToEntity(), MathS.Mod(x, 3));

        [Fact]
        public void ItStringizesBackToWhatWasParsed() =>
            Assert.Equal("x % 3", "x % 3".ToEntity().Stringize());

        [Fact]
        public void ItLatexisesAsBmod() =>
            Assert.Equal(@"x \bmod 3", "x % 3".ToEntity().Latexise());

        [Theory]
        [InlineData("x % x", "0")]
        [InlineData("(x + 1) % (x + 1)", "0")]
        [InlineData("0 % x", "0")]
        [InlineData("(x % 3) % 3", "x % 3")]
        public void TheIdentitiesThatHoldWhereverTheNodeIsDefined(string expression, string expected)
        {
            var simplified = expression.ToEntity().Simplify();
            // The rewrites carry the condition that the divisor is not zero, which is the
            // node's own condition and not something the rewrite introduced.
            while (simplified is Entity.Providedf(var inner, _)) simplified = inner;
            Assert.Equal(expected.ToEntity(), simplified);
        }

        /// <summary>
        /// x % 1 is 0 only for whole x -- 2.5 % 1 is 0.5 -- so it must not be reduced for a
        /// variable that could be anything.
        /// </summary>
        [Fact]
        public void ItIsNotReducedWhereTheIdentityDoesNotHold() =>
            Assert.Equal("x % 1".ToEntity(), "x % 1".ToEntity().Simplify());

        [Fact]
        public void SubstitutionGoesThrough() =>
            Assert.Equal(Entity.Number.Integer.Create(1), "x % 3".ToEntity().Substitute("x", 10).Evaled);

        /// <summary>
        /// a % b is a - b * floor(a / b), so where b does not depend on the variable the
        /// derivative is the dividend's own, away from the jumps. Where b does depend on it,
        /// nothing is claimed: this library has no floor node, so the general case could only
        /// be written as something that is wrong at the jumps.
        /// </summary>
        [Theory]
        [InlineData("x % 3", "1")]
        [InlineData("x ^ 2 % 3", "2 * x")]
        public void TheDerivativeIsTheDividendsWhereTheDivisorIsConstant(string expression, string expected) =>
            Assert.Equal(expected.ToEntity(), expression.ToEntity().Differentiate("x").Simplify());

        [Fact]
        public void TheDerivativeIsLeftAloneWhereTheDivisorMoves() =>
            Assert.IsType<Entity.Derivativef>("5 % x".ToEntity().Differentiate("x").Simplify());

        /// <summary>
        /// The remainder is continuous except where the dividend reaches a non-zero multiple of
        /// the divisor. Zero is not one of those: the remainder takes the dividend's sign, so
        /// x % 3 is x on either side of 0.
        /// </summary>
        [Theory]
        [InlineData("x % 3", "2", "2")]
        [InlineData("x % 3", "0", "0")]
        [InlineData("x ^ 2 % 5", "2", "4")]
        public void ALimitAtAPointItIsContinuousAtIsTheValueThere(string expression, string destination, string expected) =>
            Assert.Equal(expected.ToEntity().Evaled,
                expression.ToEntity().Limit("x", destination.ToEntity()).Simplify().Evaled);

        /// <summary>
        /// At a jump there is no two-sided limit, and which one-sided value is taken depends on
        /// the direction the dividend crosses in rather than only on what it tends to. Left
        /// unevaluated rather than answered with the value at the point, which is one of the two
        /// one-sided limits and neither the other nor the two-sided one. It must also terminate:
        /// a node with no reading of its own sends the descent back through Limitf's own
        /// evaluation, and that is a cycle.
        /// </summary>
        [Theory]
        [InlineData("x % 3", "3")]
        [InlineData("x % 3", "+oo")]
        public void AJumpIsLeftUnevaluatedAndTerminates(string expression, string destination)
        {
            var task = System.Threading.Tasks.Task.Run(
                () => expression.ToEntity().Limit("x", destination.ToEntity()).Simplify());
            Assert.True(task.Wait(TimeSpan.FromSeconds(30)), "the limit did not terminate");
            Assert.IsType<Entity.Limitf>(task.Result);
        }

        /// <summary>
        /// Both compilers, against the interpreter. The stack machine carries every value as a
        /// complex number, so it answers NaN where the two parts are not real -- the same
        /// refusal the interpreter makes by leaving the node alone.
        /// </summary>
        [Theory]
        [InlineData(7, 1)]
        [InlineData(-7, -1)]
        [InlineData(6, 0)]
        public void TheStackMachineAgreesWithTheInterpreter(int at, int expected)
        {
            var compiled = "x % 3".ToEntity().Compile("x");
            Assert.Equal(expected, compiled.Call(new Complex(at, 0)).Real, 9);
        }

        [Fact]
        public void TheStackMachineRefusesANonRealArgument() =>
            Assert.True(double.IsNaN("x % 3".ToEntity().Compile("x").Call(new Complex(2, 1)).Real));

        [Theory]
        [InlineData(7, 1)]
        [InlineData(-7, -1)]
        public void TheLinqCompilerAgreesWithTheInterpreter(int at, int expected) =>
            Assert.Equal(expected, "x % 3".ToEntity().Compile<int, int>("x")(at));

        [Fact]
        public void TheLinqCompilerKeepsTheFractionalPart() =>
            Assert.Equal(1.5, "x % y".ToEntity().Compile<double, double, double>("x", "y")(7.5, 2), 9);

        /// <summary>
        /// x % a = value has one solution per period, so answering it means introducing an
        /// integer parameter the way the trigonometric inversions do. Until that is written, no
        /// solutions is the honest answer, and a wrong one would be worse. Pinned so that
        /// whoever writes it sees this change.
        /// </summary>
        [Fact]
        public void SolvingIsNotClaimed() =>
            Assert.Empty(("x % 3".ToEntity() - 1).SolveEquation("x").DirectChildren);
    }
}
