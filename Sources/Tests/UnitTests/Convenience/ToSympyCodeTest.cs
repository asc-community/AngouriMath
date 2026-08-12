//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace AngouriMath.Tests.Convenience
{
    /// <summary>
    /// <see cref="MathS.ToSympyCode(Entity)"/> is documented as generating code you can run in
    /// SymPy, so code that cannot run is the whole of the defect.
    /// </summary>
    /// <remarks>
    /// Nothing here executes Python — the suite cannot depend on an interpreter — so these check
    /// the two properties that made the generated programs fail without one:
    /// <list type="bullet">
    /// <item>the parentheses balance, which <c>sympy.Rational(1, 2</c> did not;</item>
    /// <item>every name the body mentions is either declared in the preamble or reached through
    /// <c>sympy.</c>, which a bare <c>NaN</c> or <c>+oo</c> was not.</item>
    /// </list>
    /// https://github.com/asc-community/AngouriMath/issues/909
    /// </remarks>
    [Trait("Area", "Convenience")]
    public sealed class ToSympyCodeTest
    {
        private static (IReadOnlySet<string> Declared, string Body) Split(string code)
        {
            var declared = new HashSet<string>();
            string body = "";
            foreach (var line in code.Split('\n'))
            {
                var declaration = Regex.Match(line, @"^(\S+) = sympy\.Symbol\(");
                if (declaration.Success)
                    declared.Add(declaration.Groups[1].Value);
                else if (line.StartsWith("expr = "))
                    body = line.Substring("expr = ".Length);
            }
            return (declared, body);
        }

        /// <summary>
        /// Every name in the emitted body is bound: declared as a symbol above, or qualified with
        /// <c>sympy.</c>. A bare name is a <c>NameError</c> waiting to happen, and that is how
        /// <c>NaN</c>, <c>+oo</c> and <c>-oo</c> used to leave here.
        /// </summary>
        [Theory]
        [InlineData("0/0")]
        [InlineData("1/0")]
        [InlineData("-1/0")]
        [InlineData("+oo")]
        [InlineData("-oo")]
        [InlineData("+oo + 1")]
        [InlineData("NaN")]
        [InlineData("x + 1/2")]
        [InlineData("sqrt(2) + pi")]
        [InlineData("sin(x) / 2")]
        [InlineData("x + y + e")]
        [InlineData("i")]
        [InlineData("2 + 3 * i")]
        public void EveryNameInTheEmittedBodyIsBound(string expression)
        {
            var (declared, body) = Split(MathS.ToSympyCode(expression.ToEntity().Simplify()));
            // Whatever is reached through sympy. is bound by the import, so take those out first
            // and require the rest to have been declared.
            var unqualified = Regex.Replace(body, @"sympy\.\w+", " ");
            var loose = Regex.Matches(unqualified, @"[A-Za-z_]\w*")
                .Select(match => match.Value)
                .Where(name => !declared.Contains(name))
                .Distinct()
                .ToArray();
            Assert.True(loose.Length == 0,
                $"{expression} emitted `{body}`, which mentions {string.Join(", ", loose)} "
                + "without binding it");
        }

        /// <summary>
        /// The parentheses balance. <c>Rational</c>'s exporter was missing its closing one, so any
        /// expression carrying a non-integer rational -- most of what a CAS hands back -- emitted
        /// `SyntaxError: '(' was never closed`.
        /// </summary>
        [Theory]
        [InlineData("1/2")]
        [InlineData("1/3 + 1/6")]
        [InlineData("x + 1/2")]
        [InlineData("2/3 * x ^ (1/2)")]
        [InlineData("sin(x) / 2 + 1/4")]
        public void TheEmittedCodeHasBalancedParentheses(string expression)
        {
            var code = MathS.ToSympyCode(expression.ToEntity().Simplify());
            Assert.Equal(code.Count(character => character == '('),
                         code.Count(character => character == ')'));
        }

        /// <summary>
        /// And a rational keeps its exactness, which is the reason to emit
        /// <c>sympy.Rational</c> rather than a division of two Python integers: <c>1 / 2</c> is
        /// <c>0.5</c> there, a float.
        /// </summary>
        [Theory]
        [InlineData("1/2", "sympy.Rational(1, 2)")]
        [InlineData("1/3 + 1/6", "sympy.Rational(1, 2)")]
        [InlineData("0/0", "sympy.nan")]
        [InlineData("+oo", "sympy.oo")]
        [InlineData("-oo", "-sympy.oo")]
        public void AValueIsEmittedWithSympysOwnSpelling(string expression, string expected) =>
            Assert.Contains(expected, MathS.ToSympyCode(expression.ToEntity().Simplify()));

        /// <summary>
        /// Python's <c>/</c>, and its <c>**</c> with a negative exponent, are float operations on
        /// two integers. So the emitted body must never combine two plain integer literals with
        /// either: <c>1 / 2</c> is <c>0.5</c> there, and an exact value would leave here inexact
        /// with nothing downstream able to recover it.
        /// </summary>
        /// <remarks>
        /// Asserted as a property of the emitted text rather than by running it, since the suite
        /// cannot depend on an interpreter. These are taken <em>unsimplified</em> on purpose: a
        /// simplified <c>1/2</c> is a <c>Rational</c> node and was always exact, while what a
        /// caller writes parses to a <c>Divf</c> of two integers — which is #873 — and that is the
        /// shape that was losing the value.
        /// https://github.com/asc-community/AngouriMath/issues/911
        /// </remarks>
        [Theory]
        [InlineData("1/2")]
        [InlineData("4/2")]
        [InlineData("x + 1/2")]
        [InlineData("2 ^ (-1)")]
        [InlineData("2 ^ (-3)")]
        [InlineData("(1/2) ^ (-1)")]
        [InlineData("1/3 + 1/6")]
        [InlineData("sin(x) / 2 + 1/4")]
        public void NoTwoIntegerLiteralsAreCombinedInexactly(string expression)
        {
            var (_, body) = Split(MathS.ToSympyCode(expression.ToEntity()));
            Assert.DoesNotMatch(@"(?<![\w.)])\d+\s*/\s*\d", body);
            Assert.DoesNotMatch(@"(?<![\w.)])\d+\s*\*\*\s*\(-", body);
        }

        /// <summary>
        /// What that looks like: one operand handed to SymPy, which then does the arithmetic and
        /// keeps it exact. Only the pair-of-integers shapes are touched — with a symbol anywhere
        /// SymPy's operators already take over, and <c>2 ** 70</c> is exact because Python's
        /// integers are unbounded.
        /// </summary>
        [Theory]
        [InlineData("1/2", "sympy.Integer(1) / 2")]
        [InlineData("2 ^ (-1)", "sympy.Integer(2) ** (-1)")]
        [InlineData("x / 2", "x / 2")]
        [InlineData("1/x", "1 / x")]
        [InlineData("2 ^ 70", "2 ** 70")]
        [InlineData("x ^ (-1)", "x ** (-1)")]
        public void OnlyAPairOfIntegersIsRewritten(string expression, string expected) =>
            Assert.Contains(expected, MathS.ToSympyCode(expression.ToEntity()));
    }
}
