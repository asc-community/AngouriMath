//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

// This file is auto-generated. Use generate_additional_extensions_tests.bat to re-generate it, do not edit the file itself.

using Xunit;
using AngouriMath;
using AngouriMath.Extensions;

namespace AngouriMath.Tests.Extensions
{
    [Trait("Area", "Convenience")]
    public class IntervalExtensionTest
    {
        [Fact] public void Test0()
            => Assert.Equal(
            MathS.Interval(3, 3),
            (3, 3).ToInterval()
            );

        [Fact] public void Test0_custom()
            => Assert.Equal(
            MathS.Interval(3, true, 3, false), 
            (3, true, 3, false).ToInterval()
            );

        [Fact] public void Test1()
            => Assert.Equal(
            MathS.Interval(3, 4.5),
            (3, 4.5).ToInterval()
            );

        [Fact] public void Test1_custom()
            => Assert.Equal(
            MathS.Interval(3, true, 4.5, false), 
            (3, true, 4.5, false).ToInterval()
            );

        [Fact] public void Test2()
            => Assert.Equal(
            MathS.Interval(3, "6"),
            (3, "6").ToInterval()
            );

        [Fact] public void Test2_custom()
            => Assert.Equal(
            MathS.Interval(3, true, "6", false), 
            (3, true, "6", false).ToInterval()
            );

        [Fact] public void Test3()
            => Assert.Equal(
            MathS.Interval(4.5, 3),
            (4.5, 3).ToInterval()
            );

        [Fact] public void Test3_custom()
            => Assert.Equal(
            MathS.Interval(4.5, true, 3, false), 
            (4.5, true, 3, false).ToInterval()
            );

        [Fact] public void Test4()
            => Assert.Equal(
            MathS.Interval(4.5, 4.5),
            (4.5, 4.5).ToInterval()
            );

        [Fact] public void Test4_custom()
            => Assert.Equal(
            MathS.Interval(4.5, true, 4.5, false), 
            (4.5, true, 4.5, false).ToInterval()
            );

        [Fact] public void Test5()
            => Assert.Equal(
            MathS.Interval(4.5, "6"),
            (4.5, "6").ToInterval()
            );

        [Fact] public void Test5_custom()
            => Assert.Equal(
            MathS.Interval(4.5, true, "6", false), 
            (4.5, true, "6", false).ToInterval()
            );

        [Fact] public void Test6()
            => Assert.Equal(
            MathS.Interval("6", 3),
            ("6", 3).ToInterval()
            );

        [Fact] public void Test6_custom()
            => Assert.Equal(
            MathS.Interval("6", true, 3, false), 
            ("6", true, 3, false).ToInterval()
            );

        [Fact] public void Test7()
            => Assert.Equal(
            MathS.Interval("6", 4.5),
            ("6", 4.5).ToInterval()
            );

        [Fact] public void Test7_custom()
            => Assert.Equal(
            MathS.Interval("6", true, 4.5, false), 
            ("6", true, 4.5, false).ToInterval()
            );

        [Fact] public void Test8()
            => Assert.Equal(
            MathS.Interval("6", "6"),
            ("6", "6").ToInterval()
            );

        [Fact] public void Test8_custom()
            => Assert.Equal(
            MathS.Interval("6", true, "6", false), 
            ("6", true, "6", false).ToInterval()
            );


    }
}