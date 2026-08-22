//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using Xunit;

namespace AngouriMath.Tests.Convenience
{
    [Trait("Area", "Convenience")]
    public class ToSymPyTest
    {
        [Theory]
        [InlineData("x + 2", "import sympy")]
        [InlineData("x + 2", "Symbol('x')")]
        [InlineData("x / 2", "Symbol('x')")]
        [InlineData("e pi x / 2", "Symbol('e')", false)]
        [InlineData("e pi x / 2", "Symbol('pi')", false)]
        [InlineData("i", "sympy.I")]
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
        [InlineData("arcsin(x)", "sympy.asin(x)")]
        [InlineData("arccos(x)", "sympy.acos(x)")]
        [InlineData("arctan(x)", "sympy.atan(x)")]
        [InlineData("arccotan(x)", "sympy.acot(x)")]
        [InlineData("derivative(y, x, 2)", "sympy.diff(y, x, 2)")]
        [InlineData("integral(y, x)", "sympy.integrate(y, x)")]
        [InlineData("integral(y, x, a, b)", "sympy.integrate(y, (x, a, b))")]
        [InlineData("limit(y, x, 2)", "sympy.limit(y, x, 2)")]
        [InlineData("limitleft(y, x, 2)", "sympy.limit(y, x, 2, '-')")]
        [InlineData("limitright(y, x, 2)", "sympy.limit(y, x, 2, '+')")]
        [InlineData("sgn(y)", "sympy.sign(y)")]
        [InlineData("abs(y)", "sympy.Abs(y)")]
        [InlineData("phi(y)", "sympy.totient(y)")]
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
        // both of these recorded the old output, and neither of those outputs ran:
        // `a in B` raises rather than answering, and `ConditionSet`/`S` are unqualified.
        // https://github.com/asc-community/AngouriMath/issues/985
        [InlineData(@"a in B", "(B).contains(a)")]
        [InlineData("domain({ x : x > 0 }, RR)", "sympy.ConditionSet(x, x > 0, sympy.S.Reals)")]
        [InlineData("sec(x)", "sympy.sec(x)")]
        [InlineData("csc(x)", "sympy.csc(x)")]
        [InlineData("arcsec(x)", "sympy.asec(x)")]
        [InlineData("arccsc(x)", "sympy.acsc(x)")]
        [InlineData("pi + e", "sympy.pi + sympy.E")]
        [InlineData("1i", "1 * sympy.I")]
        [InlineData("2i", "2 * sympy.I")]
        [InlineData("3 + 2i", "3 + 2 * sympy.I")]
        [InlineData("3 + i", "3 + 1 * sympy.I")]
        [InlineData("piecewise(a provided b, c provided d)", @"Piecewise((a, b), (c, d))")]
        [InlineData("[1 , 2 , 3]", "ImmutableMatrix")]
        [InlineData("[1 , 2 , 3]", "ImmutableMatrix([1, 2, 3])")]
        [InlineData("[1 , 2 , 3]T", "ImmutableMatrix([[1, 2, 3]])")]
        public void TestSymPy(string expression, string expectedToBeIn, bool contains = true)
        {
            var ent = MathS.FromString(expression);
            if (contains)
                Assert.Contains(expectedToBeIn, MathS.ToSympyCode(ent));
            else
                Assert.DoesNotContain(expectedToBeIn, MathS.ToSympyCode(ent));
        }

        // Substring containment is what let these ship. `Assert.Contains("Piecewise((a, b),
        // (c, d))")` passes whether the parts were exported or merely interpolated, because a
        // bare variable spells the same in both languages -- and it passes on a program that
        // does not run at all, because `import sympy` alone binds neither `FiniteSet` nor `S`.
        // These pin the whole emitted expression instead. The harness that actually executes
        // it is `work/sympycheck` in the analysis workspace.
        // https://github.com/asc-community/AngouriMath/issues/985
        [Theory]
        // named without the qualifier the preamble's lone `import sympy` requires: NameError
        [InlineData("{ 1, 2 }", "sympy.FiniteSet(1, 2)")]
        [InlineData("[0; 1]", "sympy.Interval(0, 1, left_open=False, right_open=False)")]
        [InlineData("ZZ", "sympy.S.Integers")]
        [InlineData(@"{1,2} \/ {3}", "sympy.Union(sympy.FiniteSet(1, 2), sympy.FiniteSet(3))")]
        [InlineData(@"{1,2} /\ {2}", "sympy.Intersection(sympy.FiniteSet(1, 2), sympy.FiniteSet(2))")]
        [InlineData(@"{1,2} \ {2}", "sympy.Complement(sympy.FiniteSet(1, 2), sympy.FiniteSet(2))")]
        // a body that was never emitted: sympy.Lambda(x, ) is a SyntaxError
        [InlineData("lambda(x, sin(x) + 1)", "sympy.Lambda(x, sympy.sin(x) + 1)")]
        // a set builder threw AngouriBugException out of the exporter, because its codomain is
        // Domain.Any and SpecialSet.Create has no member for it
        [InlineData("{ x : x > 0 }", "sympy.ConditionSet(x, x > 0, sympy.S.UniversalSet)")]
        // parts interpolated as this library spells them rather than as SymPy does -- visible
        // only once the part is a function, since a bare name spells the same either way
        [InlineData("[sin(a); b]", "sympy.Interval(sympy.sin(a), b, left_open=False, right_open=False)")]
        [InlineData("piecewise(sin(x) provided x > 0, 1 provided x < 0)",
            "sympy.Piecewise((sympy.sin(x), x > 0), (1, x < 0))")]
        [InlineData("[[sin(a), b], [c, d]]", "sympy.ImmutableMatrix([[sympy.sin(a), b], [c, d]])")]
        // Python's `in` forces a bool, so `x in sympy.S.Reals` raises rather than answering
        // with the condition
        [InlineData("x in RR", "(sympy.S.Reals).contains(x)")]
        public void TheWholeEmittedExpressionIsWhatItShouldBe(string expression, string expected)
            => Assert.Equal(expected, EmittedExpressionOf(MathS.FromString(expression)));

        private static string EmittedExpressionOf(Entity entity)
        {
            var code = MathS.ToSympyCode(entity);
            var at = code.LastIndexOf("expr = ", System.StringComparison.Ordinal);
            return code.Substring(at + "expr = ".Length).Trim();
        }

        // The preamble is `import sympy` and nothing else, so every SymPy name in the body has
        // to carry the qualifier. Nothing was checking that, and six exports did not.
        [Theory]
        [InlineData("{ 1, 2 }")]
        [InlineData("[0; 1]")]
        [InlineData("RR")]
        [InlineData(@"{1,2} \/ {3}")]
        [InlineData("x in RR")]
        [InlineData("lambda(x, sin(x))")]
        [InlineData("piecewise(sin(x) provided x > 0, 1 provided x < 0)")]
        public void EverySymPyNameIsQualified(string expression)
        {
            var code = EmittedExpressionOf(MathS.FromString(expression));
            foreach (var name in new[]
                { "FiniteSet", "Interval", "Union", "Intersection", "Complement",
                  "ConditionSet", "Lambda", "Piecewise", "ImmutableMatrix", "S.", "sin" })
                foreach (System.Text.RegularExpressions.Match match in
                    System.Text.RegularExpressions.Regex.Matches(
                        code, System.Text.RegularExpressions.Regex.Escape(name)))
                    Assert.True(
                        match.Index >= "sympy.".Length
                        && code.Substring(match.Index - "sympy.".Length, "sympy.".Length) == "sympy.",
                        $"`{name}` is unqualified in `{code}`");
        }
    }
}
