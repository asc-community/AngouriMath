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
    public sealed class SetSimplify
    {
        [Theory]
        [InlineData("{ x : x in A }", "A")]
        [InlineData("(a + 1) in [a; a + 2]", "true")]
        [InlineData("a in (a / 2; 2a)", "a > 0")]
        [InlineData("x2 in [x2; x2 + 1]", "true")]
        [InlineData("x2 in (x2 + 1/2; x2 + 1]", "false")]
        [InlineData("x2 in (x2 - 1/2; x2 + 1]", "true")]
        [InlineData("domain((-oo; +oo), RR)", "RR")]
        [InlineData("1 in {2}", "false")]
        [InlineData("1 in {1, 2, a}", "true")]
        [InlineData("3 in {1, 2, a}", "3 in {1, 2, a}")]
        [InlineData("a in {1, 2, a}", "true")]
        [InlineData("x in {a}", "x = a")]
        [InlineData("4 in {1, 2, 3}", "false")]
        [InlineData("{3} in {1, 2, 3}", "false")]
        [InlineData("{3} in {1, 2, a}", "{3} in {1, 2, a}")]
        [InlineData("{3} in {1, 2, {3}}", "true")]
        [InlineData("{ x : x in RR } in (-oo; +oo)", "false")]
        public void TestSimplify(string unsimplified, string simplified)
        {
            var actual = unsimplified.ToEntity();
            actual = actual.InnerSimplified;
            actual = actual.Simplify();
            Assert.Equal(simplified.ToEntity(), actual);
        }
        [Theory]
        [InlineData("{ 0/0 }", "{}")]
        [InlineData("{ 1, -1/0, 0/0, 1/0, 2 }", "{ 1, 2 }")]
        [InlineData("{ 1 provided false }", "{}")]
        [InlineData("{ 1 provided true, 2 provided false, 3 provided true }", "{ 1, 3 }")]
        public void TestNaN(string unsimplified, string simplified)
        {
            var actual = unsimplified.ToEntity();
            actual = actual.InnerSimplified;
            Assert.Equal(simplified.ToEntity(), actual);
        }
    }
}
