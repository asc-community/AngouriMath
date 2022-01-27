//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

/* Copyright (c) 2019-2020 Angourisoft
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,
 * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software
 * is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

// This file is auto-generated. Use generate_additional_extensions_tests.bat to re-generate it, do not edit the file itself.

using Xunit;
using AngouriMath;
using AngouriMath.Extensions;

namespace AngouriMath.Tests.Extensions
{
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