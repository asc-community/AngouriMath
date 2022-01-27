//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using PeterO.Numbers;
using AngouriMath.Core.Exceptions;
using Xunit;
using System.Numerics;
using static AngouriMath.Entity;

namespace AngouriMath.Tests.Core
{
    public sealed class UserInvalidExceptions
    {
        [Fact] public void PrecisionTooHigh() => 
            Assert.Throws<InvalidNumberException>(() =>
            {
                using var _ = MathS.Settings.DecimalPrecisionContext.Set(
                    new EContext(EInteger.FromString("9184210498091248901840921124"), ERounding.HalfDown, EInteger.FromString("9184210498091248901840921124"), EInteger.FromString("9184210498091248901840921124"), true)
                    );
                return (BigInteger)(Number)"98492148914";
            });

        [Fact] public void InvalidNumberOfArguments1() =>
            Assert.Throws<WrongNumberOfArgumentsException>(() =>
                MathS.SolveBooleanTable("a + b", "a"));

        [Fact] public void InvalidNumberOfArguments2() =>
            Assert.Throws<WrongNumberOfArgumentsException>(() =>
                "x + 2 + y".Compile("x", "y").Call(3));

        [Fact] public void UncompilableNode1() =>
            Assert.Throws<UncompilableNodeException>(() =>
                "x + derivative(x, x)".Compile("x"));

        [Fact] public void UncompilableNode2() =>
            Assert.Throws<UncompilableNodeException>(() =>
                "x + integral(x, x)".Compile("x"));

        [Fact] public void UncompilableNode3() =>
            Assert.Throws<UncompilableNodeException>(() =>
                "x + limit(x, x, x)".Compile("x"));

        [Fact] public void UncompilableNode4() =>
            Assert.Throws<UncompilableNodeException>(() =>
                "x + { x, x }".Compile("x"));

        [Fact] public void CannotEvalNum1() =>
            Assert.Throws<CannotEvalException>(() =>
                "x".EvalNumerical());

        [Fact] public void CannotEvalNum2() =>
            Assert.Throws<CannotEvalException>(() =>
                "a e".EvalNumerical());

        [Fact] public void CannotEvalBool1() =>
            Assert.Throws<CannotEvalException>(() =>
                "a".EvalNumerical());

        [Fact] public void CannotEvalBool2() =>
            Assert.Throws<CannotEvalException>(() =>
                "a and true".EvalNumerical());

        [Fact] public void CannotEvalBool3() =>
            Assert.Throws<CannotEvalException>(() =>
                "a and false".EvalNumerical());

        [Fact] public void SetAmbiguous1() =>
            Assert.Throws<ElementInSetAmbiguousException>(() =>
                ((Set)"[a; 1.5a]").Contains("1"));

        [Fact] public void SetAmbiguous2() =>
            Assert.Throws<ElementInSetAmbiguousException>(() =>
                ((Set)"[a; 1]").Contains("b"));

        [Fact] public void SetAmbiguous3() =>
            Assert.Throws<ElementInSetAmbiguousException>(() =>
                ((Set)"[2; 3]").Contains("a"));

        [Fact] public void SetAmbiguous4() =>
            Assert.Throws<ElementInSetAmbiguousException>(() =>
                ((Set)"{ x : x > b }").Contains("a"));

        [Fact] public void SetAmbiguous5() =>
            Assert.Throws<ElementInSetAmbiguousException>(() =>
                ((Set)@"RR \ ZZ").Contains("a"));

        [Fact] public void StatementToBeTrue1() =>
            Assert.Throws<SolveRequiresStatementException>(() =>
                    "x + 2".Solve("x")
                );

        [Fact] public void StatementToBeTrue2() =>
            Assert.Throws<SolveRequiresStatementException>(() =>
                    "derivative(x, a)".Solve("x")
                );

        [Fact] public void StatementToBeTrue3() =>
            Assert.Throws<SolveRequiresStatementException>(() =>
                    "a + { x, 3 }".Solve("x")
                );

        [Fact] public void StatementToBeTrue4() =>
            Assert.Throws<SolveRequiresStatementException>(() =>
                    "((a > b) and (b > a)) + 3".Solve("x")
                );
    }
}
