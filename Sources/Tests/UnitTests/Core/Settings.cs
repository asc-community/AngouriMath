//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using Xunit;
using static AngouriMath.MathS.Settings;

namespace AngouriMath.Tests.Core
{
    public sealed class Settings
    {
        [Fact]
        public void AssertCorrectAssigning()
        {
            using var _ = MaxExpansionTermCount.Set(4);
            var actual = MaxExpansionTermCount.Value;
            Assert.Equal(4, actual);
            using var __ = MaxExpansionTermCount.Set(5);
            var actual2 = MaxExpansionTermCount.Value;
            Assert.Equal(5, actual2);
            using var ___ = MaxExpansionTermCount.Set(30902);
            var actual3 = MaxExpansionTermCount.Value;
            Assert.Equal(30902, actual3);
        }

        [Fact]
        public void AssertGetBackAfterUsing1()
        {
            using var _ = MaxExpansionTermCount.Set(10);
            using (var __ = MaxExpansionTermCount.Set(25))
                Assert.Equal(25, MaxExpansionTermCount.Value);
            Assert.Equal(10, MaxExpansionTermCount.Value);
        }

        [Fact]
        public void AssertGetBackAfterUsing2()
        {
            using var _ = MaxExpansionTermCount.Set(10);
            {
                using var __ = MaxExpansionTermCount.Set(25);
                Assert.Equal(25, MaxExpansionTermCount.Value);
            }
            Assert.Equal(10, MaxExpansionTermCount.Value);
        }

        [Fact]
        public void AssertGetBackDespiteDispose()
        {
            using var _ = MaxExpansionTermCount.Set(67);
            Assert.Equal(67, MaxExpansionTermCount.Value);
            {
                using var d1 = MaxExpansionTermCount.Set(34);
                using var d2 = MaxExpansionTermCount.Set(87);

                // normally, it is called after d2.Dispose(), but what if we call it here...
                d1.Dispose();

                // d1 was disposed, but d2 was not.
                // Since it was the last one, 
                // the setting will remain equal to d2
                Assert.Equal(87, MaxExpansionTermCount.Value);
            }
            Assert.Equal(67, MaxExpansionTermCount.Value);
        }
    }
}
