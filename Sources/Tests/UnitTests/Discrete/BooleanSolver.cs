//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace AngouriMath.Tests.Discrete
{
    using static AngouriMath.Entity;
    public sealed class BooleanSolver
    {
        [Theory]
        [InlineData(10, "A & B -> C xor D -> not B")]
        [InlineData(5, "A | C -> E")]
        [InlineData(5, "A | C implies not E")]
        [InlineData(1, "B & C and A")]
        [InlineData(9, "C and not A & B -> C xor D -> not B")]
        [InlineData(6, "C and not A | C -> E")]
        [InlineData(6, "C and not A | C implies not E")]
        [InlineData(1, "C and not B & C and A")]
        [InlineData(1, "A & B")]
        [InlineData(1, "A")]
        [InlineData(3, "A | B")]
        [InlineData(2, "A xor B")]
        [InlineData(3, "A implies B")]
        [InlineData(1, "not A")]
        public void Test(int rootNumber, string exprString)
        {
            Entity expr = exprString;
            var vars = expr.Vars.ToArray();
            var res = MathS.SolveBooleanTable(expr, vars);
            if (res is null && rootNumber == 0)
                return; // success

            // null case should explicitly quit the scope
            if (res is null)
            {
                Assert.True(false, $"0 roots instead of {rootNumber}");
                return;
            }

            Assert.Equal(rootNumber, res.RowCount);

            var dict = new Dictionary<Variable, Entity>();
            var count = vars.Length;
            for (int i = 0; i < res.RowCount; i++)
            {
                for (int j = 0; j < count; j++)
                    dict[vars[j]] = res[i, j];
                Assert.True(expr.Substitute(dict).EvalBoolean());
            }
        }

        [Theory]
        [InlineData("(x implies a) = b", "{ False provided a and b, True provided a and b, False provided not a and b, True provided not a and not b }")]
        [InlineData("(x and a) = b", "{ True provided b and a, False provided a and not b, True provided not a and not b, False provided not a and not b }")]
        [InlineData("(x or a) = b", "{ True provided a and b, False provided a and b, True provided not a and b, False provided not a and not b }")]
        [InlineData("(x xor a) = b", "{ b provided not a, not b provided a }")]
        public void TestSymbolicSolver(string statementRaw, string expectedRaw)
        {
            Entity expected = expectedRaw;
            Entity statement = statementRaw;
            var solSet = statement.Solve("x");
            Assert.Equal(expected, solSet.InnerSimplified);
        }
    }
}
