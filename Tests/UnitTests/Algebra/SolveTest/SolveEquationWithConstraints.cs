using AngouriMath;
using AngouriMath.Extensions;
using AngouriMath.Core;
using static AngouriMath.Entity.Number;
using Xunit;
using System.Collections.Generic;
using static AngouriMath.Entity;
using System.Linq;
using static AngouriMath.Entity.Set;
using System;

namespace UnitTests.Algebra
{
    public class SolveEquationWithConstraints
    {
        /// <summary>Numerically checks if a root fits an equation</summary>
        internal static void AssertRoots(Entity equation, Variable toSub, Entity varValue, Integer? subValue = null)
        {
            subValue ??= 3;
            string? eqNormal = equation.Stringize();
            equation = equation.Substitute(toSub, varValue);
            // MUST be integer to correspond to integer coefficient of periodic roots
            var substitutions = new Dictionary<Entity.Variable, Integer>();
            foreach (var vr in equation.Vars)
                substitutions.Add(vr, subValue + substitutions.Count);
            equation = equation.Substitute(substitutions);
            var err = equation.EvalBoolean();
            Assert.True(err, $"\nError = {err.Stringize()}\n{eqNormal}\nWrong root: {toSub.Stringize()} = {varValue.Stringize()}");
        }

        [Theory]
        [InlineData("x + 2 = 0", 1)]
        [InlineData("x2 = 16", 2)]
        [InlineData("x2 = 16 and x = 4", 1)]
        [InlineData("x2 = 16 and x = -4", 1)]
        [InlineData("x2 = 16 and x = 3", 0)]
        [InlineData("x2 = 16 or x = 3", 3)]
        [InlineData("x4 = 16 and x > 0", 1)]
        [InlineData("x4 = 0 and x > 0", 0)]
        [InlineData("x4 = 0 and x >= 0", 1)]
        [InlineData("x = 1 or x = 2 or x = 3", 3)]
        public void TestFinite(string expr, int rootCount)
        {
            var eq = expr.ToEntity();
            Variable x = "x";
            var solutions = eq.Solve(x);
            solutions = (Set)solutions.InnerSimplified;
            Assert.IsType<FiniteSet>(solutions);
            if (solutions is not FiniteSet roots)
                throw new Exception(); // C#'s analyzers can't into Assert.True's exceptions
            Assert.Equal(rootCount, roots.Count);
            foreach (var root in roots)
                AssertRoots(eq, "x", root);
        }

        [Fact(Skip = "Sets require more work")]
        public void TestFiniteIntersection() => TestFinite("x2 = a and x = a", 1);
    }
}
