//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using Xunit;
using AngouriMath;
using AngouriMath.Extensions;

namespace AngouriMath.Tests.Common
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
