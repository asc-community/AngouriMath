using AngouriMath;
using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace UnitTests.Convenience
{
    public class SynonymFunctionTest
    {
        private readonly Entity x = MathS.Var(nameof(x));

        [Theory]
        [InlineData("Sin", "sin(x)")]
        [InlineData("Cos", "cos(x)")]
        [InlineData("Tan", "tan(x)")]
        [InlineData("Cotan", "cotan(x)")]
        [InlineData("Sec", "sec(x)")]
        [InlineData("Cosec", "cosec(x)")]
        [InlineData("Arcsec", "arcsec(x)")]
        [InlineData("Arccosec", "arccosec(x)")]
        [InlineData("Arctan", "arctan(x)")]
        [InlineData("Arccotan", "arccotan(x)")]
        [InlineData("Sqr", "sqr(x)")]
        [InlineData("Sqrt", "sqrt(x)")]
        [InlineData("Ln", "ln(x)")]
        [InlineData("Log", "log(x)")]
        [InlineData("Signum", "sgn(x)")]
        [InlineData("Signum", "signum(x)")]
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
            if (!(entObj is Entity ent))
                throw new InvalidCastException($"Invokation returned {entObj?.GetType()} instead of {typeof(Entity)}");
            Assert.Equal(ent, MathS.FromString(stringizedExpr));
        }
    }
}
