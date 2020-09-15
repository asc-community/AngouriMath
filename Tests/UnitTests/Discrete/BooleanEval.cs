using AngouriMath;
using Xunit;

namespace UnitTests.Discrete
{
    using static Entity.Boolean;

    public class BooleanEval
    {
        public void Test(Entity expr, Entity.Boolean expected)
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
    }
}
