//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using System;
using System.Linq;
using System.Reflection;
using Xunit;
using AngouriMath.Extensions;

namespace AngouriMath.Tests.Convenience
{
    public sealed class SynonymFunctionTest
    {
        private readonly Entity x = MathS.Var(nameof(x));

        [Theory]
        [InlineData("Sin", "sin(x)")]
        [InlineData("Cos", "cos(x)")]
        [InlineData("Tan", "tan(x)")]
        [InlineData("Cotan", "cotan(x)")]
        [InlineData("Cotan", "cot(x)")]
        [InlineData("Arcsin", "arcsin(x)")]
        [InlineData("Arccos", "arccos(x)")]
        [InlineData("Arctan", "arctan(x)")]
        [InlineData("Arccotan", "arccotan(x)")]
        [InlineData("Arcsin", "asin(x)")]
        [InlineData("Arccos", "acos(x)")]
        [InlineData("Arctan", "atan(x)")]
        [InlineData("Arccotan", "acotan(x)")]
        [InlineData("Arccotan", "acot(x)")]
        [InlineData("Sec", "sec(x)")]
        [InlineData("Cosec", "cosec(x)")]
        [InlineData("Cosec", "csc(x)")]
        [InlineData("Arcsec", "arcsec(x)")]
        [InlineData("Arccosec", "arccosec(x)")]
        [InlineData("Arcsec", "asec(x)")]
        [InlineData("Arccosec", "acsc(x)")]
        [InlineData("Arccosec", "arccsc(x)")]
        [InlineData("Sqr", "sqr(x)")]
        [InlineData("Sqrt", "sqrt(x)")]
        [InlineData("Ln", "ln(x)")]
        [InlineData("Log", "log(x)")]
        [InlineData("Signum", "sgn(x)")]
        [InlineData("Signum", "signum(x)")]
        [InlineData("Signum", "sign(x)")]
        [InlineData("Abs", "abs(x)")]
        public void TestOneArgumentSynonym(string mathSFuncName, string stringizedExpr)
        {
            var mis = typeof(MathS)
                .GetMethods()
                .Where(mi => mi.Name == mathSFuncName)
                .Where(mi => mi.GetParameters().Length == 1)
                .Where(mi => mi.GetParameters()[0].ParameterType == typeof(Entity));
            if (mis.Count() != 1)
                throw new AmbiguousMatchException(
                    $"Can't choose a method from {string.Join("\n", mis)}"
                    );
            var mi = mis.First();

            var entObj = mi.Invoke(null, new[] { x });
            if (entObj is not Entity ent)
                throw new InvalidCastException($"Invokation returned {entObj?.GetType()} instead of {typeof(Entity)}");
            Assert.Equal(ent, MathS.FromString(stringizedExpr));
        }

        [Fact]
        public void TestExtensionToFiniteSet()
            => Assert.Equal(MathS.Sets.Finite(1, 2, 3), new Entity[] { 1, 2, 3 }.ToSet());
    }
}
