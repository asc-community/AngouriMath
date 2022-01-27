//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using static AngouriMath.Entity.Number;
using Xunit;
using PeterO.Numbers;

namespace AngouriMath.Tests.PatternsTest
{
    public sealed class SmartExpander
    {
        public (bool equal, Complex eval1, Complex eval2, Real err) AreEqual(Entity expr1, Entity expr2, Complex toSub)
        {
            foreach (var var in expr1.Vars)
                expr1 = expr1.Substitute(var, toSub);
            foreach (var var in expr2.Vars)
                expr2 = expr2.Substitute(var, toSub);
            var evaled1 = expr1.EvalNumerical();
            var evaled2 = expr2.EvalNumerical();
            return (
                evaled1 == evaled2 /*non-finite goes with that*/ 
                || (evaled1 - evaled2).Abs().EDecimal.CompareTo(EDecimal.FromDecimal(1e-20m)) < 0, 
                
                evaled1, evaled2, (evaled1 - evaled2).Abs());
        }

        void AssertExpander(Entity expr, Complex[] toSubs, bool nullExpansion = false)
        {
            using var _ = MathS.Settings.MaxExpansionTermCount.Set(3000);
            var expanded =  MathS.Utils.SmartExpandOver(expr, "x");
            foreach (var toSub in toSubs)
            {
                if (nullExpansion)
                    Assert.Equal(expr, expanded); // Nodes should be same
                else
                    Assert.NotEqual(expr, expanded); // Nodes should be different
                var (equal, expected, actual, err) = AreEqual(expr, expanded, toSub);
                Assert.True(equal, $"\nexpected: {expected.Stringize()}\nactual: {actual.Stringize()}\nerror: {err.Stringize()}\ntoSub: {toSub.Stringize()}\nexpanded: {expanded.Stringize()}");
            }
        }
        

        private readonly Complex[] TestSet1 = { 3, 6, 8, -3, MathS.i * 2 };

        [Fact] public void TestCorner1() => AssertExpander("x", TestSet1, true);
        [Fact] public void TestCorner2() => AssertExpander("3", TestSet1, true);
        [Fact] public void TestCorner3() => AssertExpander("sin(x)", TestSet1, true);
        [Fact] public void TestCorner4() => AssertExpander("1 / x", TestSet1, true);

        [Fact] public void TestPow1() => AssertExpander("(x + 2)2", TestSet1);
        [Fact] public void TestPow2() => AssertExpander("(x + a + b)3", TestSet1);
        [Fact] public void TestPow3() => AssertExpander("(x + a + b + x2)5", TestSet1);
        [Fact] public void TestPow4() => AssertExpander("(x + a + b + x2 + x3)8", TestSet1);
        [Fact] public void TestPow5() => AssertExpander("(x + a + b + x2 + x3)8", TestSet1);
        [Fact] public void TestPow6() => AssertExpander("(x + 1)0", TestSet1, true);
        [Fact] public void TestPow7() => AssertExpander("(x + 1)^(-2)", TestSet1, true);

        [Fact] public void TestMul1() => AssertExpander("(x + 1)(x + b)", TestSet1);
        [Fact] public void TestMul2() => AssertExpander("(x + 1)(x + b + c)", TestSet1);
        [Fact] public void TestMul3() => AssertExpander("(x + 1)(x + b + x3 + x4)", TestSet1);
        [Fact] public void TestMul4() => AssertExpander("(x + 1)(x + b + x3 + x4)(x + a + x2)", TestSet1);
        [Fact] public void TestCornerMul() => AssertExpander("(x + sin(a))(sin(x2) + b)", TestSet1);
        [Fact] public void Factorial1() => AssertExpander("x!/(x+3)!", TestSet1);
        [Fact] public void Factorial2() => AssertExpander("(x+16)!/(x+6)!", TestSet1);

        [Fact] public void TestDiv1() => AssertExpander("(x + 1) / (x2 + x3)", TestSet1);
        [Fact] public void TestDiv2() => AssertExpander("(x + a + b + x2 + x3)/(x + b + x3 + x4)", TestSet1);
    }
}
