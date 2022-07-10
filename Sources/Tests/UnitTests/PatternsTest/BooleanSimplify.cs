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

using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.PatternsTest
{
    public sealed class BooleanSimplify
    {
        [Theory]
        [InlineData("true and true", "true")]
        [InlineData("false implies anything", "true")]
        [InlineData("false or anything", "anything")]
        [InlineData("true or anything", "true")]
        [InlineData("anything or false", "anything")]
        [InlineData("anything or true", "true")]
        [InlineData("anything or anything", "anything")]
        [InlineData("anything and anything", "anything")]
        [InlineData("anything implies anything", "true")]
        [InlineData("not a or not b", "not (a and b)")]
        [InlineData("not a and not b", "not (a or b)")]
        [InlineData("not a or b", "a implies b")]
        [InlineData("not not a", "a")]
        [InlineData("not not not a", "not a")]
        [InlineData("a and b and a", "a and b")]
        [InlineData("a and b and b", "a and b")]
        [InlineData("a and b and c and f and a", "a and b and c and f")]
        [InlineData("a and not b or b and not a", "a xor b")]
        [InlineData("not a and b or a and not b", "a xor b")]
        [InlineData("not a and b or not b and a", "a xor b")]
        [InlineData("a and not b or not a and b", "a xor b")]
        [InlineData("b xor a xor c xor d", "a xor b xor c xor d")]
        public void Test(string expr, string expected)
        {
            var exp = expected.ToEntity();
            var act = expr.ToEntity();
            Assert.Equal(exp, act.Simplify());
            
        }

        [Theory]
        [InlineData("0 < x",  "x > 0")]
        [InlineData("0 <= x", "x >= 0")]
        [InlineData("0 > x",  "x < 0")]
        [InlineData("0 >= x", "x <= 0")]
        [InlineData("0 = x", "x = 0")]

        [InlineData("2 * x > 0", "x > 0")]
        [InlineData("2 * x < 0", "x < 0")]
        [InlineData("2 * x >= 0", "x >= 0")]
        [InlineData("2 * x <= 0", "x <= 0")]

        [InlineData("x * 2 > 0", "x > 0")]
        [InlineData("x * 2 < 0", "x < 0")]
        [InlineData("x * 2 >= 0", "x >= 0")]
        [InlineData("x * 2 <= 0", "x <= 0")]

        [InlineData("-2 * x > 0", "x < 0")]
        [InlineData("-2 * x < 0", "x > 0")]
        [InlineData("-2 * x >= 0", "x <= 0")]
        [InlineData("-2 * x <= 0", "x >= 0")]

        [InlineData("x * (-2) > 0", "x < 0")]
        [InlineData("x * (-2) < 0", "x > 0")]
        [InlineData("x * (-2) >= 0", "x <= 0")]
        [InlineData("x * (-2) <= 0", "x >= 0")]

        [InlineData("x / 2 > 0", "x > 0")]
        [InlineData("x / 2 < 0", "x < 0")]
        [InlineData("x / 2 >= 0", "x >= 0")]
        [InlineData("x / 2 <= 0", "x <= 0")]

        [InlineData("x / (-2) > 0", "x < 0")]
        [InlineData("x / (-2) < 0", "x > 0")]
        [InlineData("x / (-2) >= 0", "x <= 0")]
        [InlineData("x / (-2) <= 0", "x >= 0")]

        [InlineData("0 > 2 * x", "x < 0")]
        [InlineData("0 < 2 * x", "x > 0")]
        [InlineData("0 >= 2 * x", "x <= 0")]
        [InlineData("0 <= 2 * x", "x >= 0")]
        [InlineData("x ^ 2 = 0", "x = 0")]
        [InlineData("x > y > z -> x > z", "true")]
        [InlineData("x < y < z -> x < z", "true")]
        public void TestInequality(string expr, string expected)
        {
            var exp = expected.ToEntity();
            var act = expr.ToEntity();
            Assert.Equal(exp, act.Simplify());
        }
    }
}
