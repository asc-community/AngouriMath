using AngouriMath;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace UnitTests.Discrete
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

            Assert.Equal(rootNumber, res.Shape[0]);

            var dict = new Dictionary<Variable, Entity>();
            var count = vars.Length;
            for (int i = 0; i < res.Shape[0]; i++)
            {
                for (int j = 0; j < count; j++)
                    dict[vars[j]] = res[i, j];
                Assert.True(expr.Substitute(dict).EvalBoolean());
            }
        }
    }
}
