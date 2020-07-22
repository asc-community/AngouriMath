using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AngouriMath;
using AngouriMath.Core.Numerix;
using UnitTests.Algebra.PolynomialSolverTests;

namespace UnitTests.Algebra
{
    [TestClass]
    public class SolveSystem
    {
        public void AssertSystemSolvable(List<Entity> equations, List<VariableEntity> vars, int rootCount = -1, ComplexNumber? ToSub = null)
        {
            ToSub ??= 3;
            var sys = MathS.Equations(equations.ToArray());
            var sol = sys.Solve(vars.ToArray());
            if (rootCount != -1)
                Assert.AreEqual(rootCount, sol.Shape[0]);
            for (int i = 0; i < sol.Shape[0]; i++)
            {
                foreach (var eq in equations)
                {
                    var eqCopy = eq.DeepCopy();
                    Assert.AreEqual(sol.Shape[1], vars.Count, "Incorrect output of Solve");
                    for (int rootid = 0; rootid < sol.Shape[1]; rootid++)
                    {
                        eqCopy = eqCopy.Substitute(vars[rootid], sol[i, rootid]);
                    }

                    foreach (var uniqvar in MathS.Utils.GetUniqueVariables(eqCopy).FiniteSet())
                        eqCopy = eqCopy.Substitute(uniqvar.Name, new NumberEntity(ToSub));
                    var E = Number.Abs(eqCopy.Eval());
                    Assert.IsTrue(E.IsFinite && E < 0.0001,
                        "i: " + i + "  eq: " + eq.ToString() + "  E: " + E.ToString());
                }
            }
        }

        public List<Entity> EQ(params Entity[] equations)
            => new List<Entity>(equations);

        public List<VariableEntity> VA(params VariableEntity[] vars)
            => new List<VariableEntity>(vars);

        [TestMethod]
        public void TestLinSystem2()
        {
            var eqs = EQ(
                "x + y - 3",
                "x + 2y - 4"
                );
            AssertSystemSolvable(eqs, VA("x", "y"), 1);
        }

        [TestMethod]
        public void TestLinSystem3()
        {
            var eqs = EQ(
                "x + y - 3",
                "x + 2y - 4z",
                "z + 3x - 0.1y - 1"
            );
            AssertSystemSolvable(eqs, VA("x", "y", "z"), 1);
        }

        [TestMethod]
        public void TestPolySystem3()
        {
            var eqs = EQ(
                "x2 + y3 + z + 4",
                "3y3 - z - 2",
                "x2 - 0.1z + 4x2 + 4y3"
                );
            var vars = VA("x", "y", "z");
            AssertSystemSolvable(eqs, vars, 6, ToSub: 5);
        }

        [TestMethod]
        public void TestTrigSystem2()
        {
            var eqs = EQ(
                "cos(x2 + 1)^2 + 3y",
                "y * (-1) + 4cos(x2 + 1)"
                );
            AssertSystemSolvable(eqs, VA("x", "y"), 8); // TODO: Should be 6 solutions according to Wolfram Alpha
        }

        [TestMethod]
        public void TestTrigSystem3()
        {
            var eqs = EQ(
                "a+b-c",
                "3a+3b-2c",
                "3a+4b-4c"
            );
            AssertSystemSolvable(eqs, VA("a", "b", "c"), 1);
        }

        [TestMethod]
        public void TestTrigSystem4()
        {
            var eqs = EQ(
                "a - 1",
                "b * 0"
            );
            AssertSystemSolvable(eqs, VA("a", "b"), 0);
        }

        [TestMethod]
        public void TestSystemFromGermanOlympiad()
        {
            // https://www.youtube.com/watch?v=dVs26SSUJSA
            var eqs = EQ(
                "x3 + 9 x2 y - 10",
                "y3 + x y2 - 2"
            );
            AssertSystemSolvable(eqs, VA("x", "y"), 9);
        }

        [TestMethod]
        public void TestSystemFromSomewhereHmmAnalytical()
        {
            var eqs = EQ(
                "x3 - 9 x2 y - f",
                "y3 + x y2 - a"
            );
            AssertSystemSolvable(eqs, VA("x", "y"), 9);
        }
    }
}
