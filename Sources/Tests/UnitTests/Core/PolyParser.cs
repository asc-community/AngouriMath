//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using Xunit;

namespace AngouriMath.Tests.Core
{
    public class PolyParser
    {
        // Workaround to avoid ToString which results in SO in RC1
        private void AssertEntityEqual(Entity a, Entity b)
        {
            b = b.InnerSimplified;
            if (a != b)
                Assert.False(true, $"{a.Stringize()} is not {b.Stringize()}");
        }

        private void Fail()
        {
            Assert.True(false, "Not parsed for some reason");
        }

        [Fact]
        public void TestLinear1()
        {
            if (MathS.Utils.TryGetPolyLinear("a x + b", "x", out var a, out var b))
            {
                AssertEntityEqual("a", a);
                AssertEntityEqual("b", b);
            }
            else
                Fail();
        }

        [Fact]
        public void TestLinear2()
        {
            if (MathS.Utils.TryGetPolyLinear("a x - b", "x", out var a, out var b))
            {
                AssertEntityEqual("a", a);
                AssertEntityEqual("-b", b);
            }
            else
                Fail();
        }

        [Fact]
        public void TestLinear3()
        {
            if (MathS.Utils.TryGetPolyLinear("a x - b + 3x + x", "x", out var a, out var b))
            {
                AssertEntityEqual("a + 3 + 1", a);
                AssertEntityEqual("-b", b);
            }
            else
                Fail();
        }

        [Fact]
        public void TestLinear4()
        {
            Assert.False(MathS.Utils.TryGetPolyLinear("a x - b + 3x + x2", "x", out _, out _));
        }

        [Fact]
        public void TestLinear5()
        {
            Assert.False(MathS.Utils.TryGetPolyLinear("a x - b + 3x + x^(-999)", "x", out _, out _));
        }

        [Fact]
        public void TestQuadratic1()
        {
            if (MathS.Utils.TryGetPolyQuadratic("a x2 + b x + c", "x", out var a, out var b, out var c))
            {
                AssertEntityEqual("a", a);
                AssertEntityEqual("b", b);
                AssertEntityEqual("c", c);
            }
            else
                Fail();
        }

        [Fact]
        public void TestQuadratic2()
        {
            if (MathS.Utils.TryGetPolyQuadratic("a x2 + b x2 + c", "x", out var a, out var b, out var c))
            {
                AssertEntityEqual("a + b", a);
                AssertEntityEqual("0", b);
                AssertEntityEqual("c", c);
            }
            else
                Fail();
        }
    }
}
