using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AngouriMath;
using AngouriMath.Core.Numerix;

namespace UnitTests.Algebra
{
    [TestClass]
    public class SolveSystem
    {
        public void AssertSystemSolvable(Entity[] equations, Entity.Variable[] vars, int rootCount, IntegerNumber? ToSub = null)
        {
            ToSub ??= 3;
            var sol = MathS.Equations(equations).Solve(vars);
            if (sol is null)
                if (rootCount == 0)
                    return;
                else throw new AssertFailedException($"{nameof(sol)} is null but {nameof(rootCount)} is {rootCount}");
            if (rootCount != -1)
                Assert.AreEqual(rootCount, sol.Shape[0]);
            var substitutions = new Dictionary<Entity.Variable, Entity>();
            for (int i = 0; i < sol.Shape[0]; i++)
                foreach (var equation in equations)
                {
                    var eqCopy = equation;
                    Assert.AreEqual(sol.Shape[1], vars.Length, "Incorrect output of Solve");
                    substitutions.Clear();
                    for (int rootid = 0; rootid < sol.Shape[1]; rootid++)
                        substitutions.Add(vars[rootid], sol[i, rootid]);
                    eqCopy = eqCopy.Substitute(substitutions);

                    substitutions.Clear();
                    foreach (var uniqvar in eqCopy.Vars)
                        substitutions.Add(uniqvar, ToSub);
                    eqCopy = eqCopy.Substitute(substitutions);
                    var error = Entity.Number.Abs(eqCopy.Eval());
                    Assert.IsTrue(error.IsFinite && error < 0.0001,
                        $"\n{nameof(equation)}: {equation.InnerSimplify()}\n{nameof(i)}: {i}\n{nameof(error)}: {error}\n{nameof(sol)}: {sol}");
                }
        }

        [TestMethod]
        public void TestLinSystem2() => AssertSystemSolvable(new Entity[] {
            "x + y - 3",
            "x + 2y - 4"
        }, new Entity.Variable[] { "x", "y" }, 1);

        [TestMethod]
        public void TestLinSystem3() => AssertSystemSolvable(new Entity[] {
            "x + y - 3",
            "x + 2y - 4z",
            "z + 3x - 0.1y - 1"
        }, new Entity.Variable[] { "x", "y", "z" }, 1);

        [TestMethod]
        public void TestPolySystem3() => AssertSystemSolvable(new Entity[] {
            "x2 + y3 + z + 4",
            "3y3 - z - 2",
            "x2 - 0.1z + 4x2 + 4y3"
        }, new Entity.Variable[] { "x", "y", "z" }, 6, 5);

        [TestMethod]
        public void TestTrigSystem2() => AssertSystemSolvable(new Entity[] {
            "cos(x2 + 1)^2 + 3y",
            "y * (-1) + 4cos(x2 + 1)"
        }, new Entity.Variable[] { "x", "y" }, 8); // TODO: Should be 6 solutions according to Wolfram Alpha

        [TestMethod]
        public void TestTrigSystem3() => AssertSystemSolvable(new Entity[] {
            "a+b-c",
            "3a+3b-2c",
            "3a+4b-4c"
        }, new Entity.Variable[] { "a", "b", "c" }, 1);

        [TestMethod]
        public void TestTrigSystem4() => AssertSystemSolvable(new Entity[] {
            "a - 1",
            "b * 0"
        }, new Entity.Variable[] { "a", "b" }, 0);

        // https://www.youtube.com/watch?v=dVs26SSUJSA
        [TestMethod]
        public void TestSystemFromGermanOlympiad() => AssertSystemSolvable(new Entity[] {
            "x3 + 9 x2 y - 10",
            "y3 + x y2 - 2"
        }, new Entity.Variable[] { "x", "y" }, 9);

        [TestMethod] // Hint: The above test :)
        public void TestSystemFromSomewhereHmmAnalytical() => AssertSystemSolvable(new Entity[] {
            "x3 - 9 x2 y - f",
            "y3 + x y2 - a"
        }, new Entity.Variable[] { "x", "y" }, 9);
    }
}
