using Xunit;
using AngouriMath;
using AngouriMath.Extensions;

namespace UnitTests.Common
{
    public static class HierarchyTestExtensions
    {
        public static bool IsOfType<T>(this string @this)
        {
            Assert.IsAssignableFrom<T>(@this.ToEntity());
            return true;
        }
    }

    public sealed class HierarchyTest
    {
        [Theory, CombinatorialData]
        public void TestContinuousNode(
            [CombinatorialValues(
            "a + b", "a - b", "a * b", "a / b", "a ^ b",
            "(|a|)", "sgn(a)"
            )]
            string expr)
            => expr.IsOfType<Entity.ContinuousNode>();

        [Theory, CombinatorialData]
        public bool TestTrigonometricNode(
            [CombinatorialValues("arcsin", "arccos", "sin", "cos",
            "tan", "cotan", "arctan", "arccotan", "sec", "cosec",
            "arcsec", "arccosec")] string node)
            => $"{node}(x)".IsOfType<Entity.ContinuousNode>() && $"{node}(x)".IsOfType<Entity.TrigonometricFunction>();
    }
}
