//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using Xunit;

namespace AngouriMath.Tests.Discrete
{
    using static Entity.Boolean;

    public sealed class BooleanEval
    {
        private void Test(Entity expr, Entity.Boolean expected)
            => Assert.Equal(expected, expr.EvalBoolean());

        [Fact] public void Test1() => Test(True, True);
        [Fact] public void Test2() => Test(False, False);
        [Fact] public void TestNegate1() => Test(!True, False);
        [Fact] public void TestNegate2() => Test(!False, True);
        [Fact] public void TestAnd1() => Test(True & True, True);
        [Fact] public void TestAnd2() => Test(True & False, False);
        [Fact] public void TestAnd3() => Test(False & True, False);
        [Fact] public void TestAnd4() => Test(False & False, False);
        [Fact] public void TestOr1() => Test(True | True, True);
        [Fact] public void TestOr2() => Test(True | False, True);
        [Fact] public void TestOr3() => Test(False | True, True);
        [Fact] public void TestOr4() => Test(False | False, False);
        [Fact] public void TestXor1() => Test(True ^ True, False);
        [Fact] public void TestXor2() => Test(True ^ False, True);
        [Fact] public void TestXor3() => Test(False ^ True, True);
        [Fact] public void TestXor4() => Test(False ^ False, False);
        [Fact] public void TestImplies1() => Test(True.Implies(True), True);
        [Fact] public void TestImplies2() => Test(False.Implies(True), True);
        [Fact] public void TestImplies3() => Test(False.Implies(False), True);
        [Fact] public void TestImplies4() => Test(True.Implies(False), False);

        [Theory]
        [InlineData("not false")]
        [InlineData("true")]
        [InlineData("not false and true")]
        [InlineData("not (not true and false)")]
        [InlineData("false -> true")]
        [InlineData("true -> true")]
        [InlineData("false and not true or true")]
        public void TestString(string expr)        
            => Assert.Equal(True, MathS.FromString(expr).EvalBoolean());
    }
}
