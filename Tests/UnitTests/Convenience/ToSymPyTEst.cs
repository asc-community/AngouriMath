using AngouriMath;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace UnitTests.Convenience
{
    public class ToSymPyTest
    {
        [Theory]
        [InlineData("x + 2", "import sympy")]
        [InlineData("x + 2", "Symbol('x')")]
        [InlineData("x / 2", "Symbol('x')")]
        [InlineData("e pi x / 2", "Symbol('e')", false)]
        [InlineData("e pi x / 2", "Symbol('pi')", false)]
        [InlineData("i", "sympy.I", false)]
        [InlineData("{ 1, 2 }", "Symbol('pi')", false)]
        [InlineData("x - 2", "x - 2")]
        [InlineData("x + 2", "x + 2")]
        [InlineData("x * 2", "x * 2")]
        [InlineData("x / 2", "x / 2")]
        [InlineData("x ^ 2", "x ** 2")]
        [InlineData("log(2, x)", "log(x, 2)")]
        [InlineData("log(2, 3)", "log(3, 2)")]
        [InlineData("sin(x)", "sympy.sin(x)")]
        [InlineData("cos(x)", "sympy.cos(x)")]
        [InlineData("cotan(x)", "sympy.cot(x)")]
        [InlineData("arcsin(x)", "sympy.arcsin(x)")]
        [InlineData("arccos(x)", "sympy.arccos(x)")]
        [InlineData("arctan(x)", "sympy.arctan(x)")]
        [InlineData("arccotan(x)", "sympy.arccot(x)")]
        [InlineData("derivative(y, x, 2)", "sympy.diff(y, x, 2)")]
        [InlineData("integral(y, x, 2)", "sympy.integrate(y, x, 2)")]
        [InlineData("limit(y, x, 2)", "sympy.limit(y, x, 2)")]
        [InlineData("limitleft(y, x, 2)", "sympy.limit(y, x, 2, '-')")]
        [InlineData("limitright(y, x, 2)", "sympy.limit(y, x, 2, '+')")]
        [InlineData("sgn(y)", "sympy.sign(y)")]
        [InlineData("abs(y)", "sympy.Abs(y)")]
        [InlineData("true", "True")]
        [InlineData("false", "False")]
        [InlineData("not a", "not a")]
        [InlineData("a and b", "a and b")]
        [InlineData("a or b", "a or b")]
        [InlineData("a xor b", "a ^ b")]
        [InlineData("a implies b", "sympy.Implies(a, b)")]
        [InlineData("a = b", "a == b")]
        [InlineData("a > b", "a > b")]
        [InlineData("a < b", "a < b")]
        [InlineData("a >= b", "a >= b")]
        [InlineData("a <= b", "a <= b")]
        [InlineData("{ 1, 2 }", "FiniteSet(1, 2)")]
        [InlineData("[a; b]", "Interval(a, b, left_open=False, right_open=False)")]
        [InlineData("[a; b)", "Interval(a, b, left_open=False, right_open=True)")]
        [InlineData("(a; b]", "Interval(a, b, left_open=True, right_open=False)")]
        [InlineData("(a; b)", "Interval(a, b, left_open=True, right_open=True)")]
        [InlineData("ZZ", "S.Integers")]
        [InlineData("QQ", "S.Rationals")]
        [InlineData("RR", "S.Reals")]
        [InlineData("CC", "S.Complexes")]
        [InlineData(@"A \/ B", "Union(A, B)")]
        [InlineData(@"A /\ B", "Intersection(A, B)")]
        [InlineData(@"A \ B", "Complement(A, B)")]
        [InlineData(@"a in B", "a in B")]
        public void TestSymPy(string expression, string expectedToBeIn, bool contains = true)
        {
            var ent = MathS.FromString(expression);
            if (contains)
                Assert.Contains(expectedToBeIn, MathS.ToSympyCode(ent));
            else
                Assert.DoesNotContain(expectedToBeIn, MathS.ToSympyCode(ent));
        }
    }
}

