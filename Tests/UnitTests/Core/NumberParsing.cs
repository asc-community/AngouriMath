using AngouriMath;
using AngouriMath.Core.Numerix;
using Xunit;

namespace UnitTests.Core
{
    public class NumberParsing
    {
        [Theory]
        [InlineData("quack")]
        [InlineData("i1")]
        [InlineData("ii")]
        [InlineData("")]
        public void NotComplex(string input) => Assert.False(ComplexNumber.TryParse(input, out _));
        [Theory]
        [InlineData("233i")]
        [InlineData("-4i")]
        [InlineData("-i")]
        [InlineData("-5.3i")]
        [InlineData("5.3")]
        public void Complex(string input) => Assert.True(ComplexNumber.TryParse(input, out _));
    }
}
