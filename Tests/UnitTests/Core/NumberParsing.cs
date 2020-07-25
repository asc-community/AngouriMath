using AngouriMath;
using AngouriMath.Core.Numerix;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Core
{
    [TestClass]
    public class NumberParsing
    {
        [TestMethod]
        public void ParseComplex1()
            => Assert.IsFalse(ComplexNumber.TryParse("quack", out _));

        [TestMethod]
        public void ParseComplex2()
            => Assert.IsFalse(ComplexNumber.TryParse("i1", out _));

        [TestMethod]
        public void ParseComplex3()
            => Assert.IsFalse(ComplexNumber.TryParse("ii", out _));

        [TestMethod]
        public void ParseComplex4()
            => Assert.IsFalse(ComplexNumber.TryParse("", out _));

        [TestMethod]
        public void ParseComplex5()
            => Assert.IsTrue(ComplexNumber.TryParse("233i", out _));

        [TestMethod]
        public void ParseComplex6()
            => Assert.IsTrue(ComplexNumber.TryParse("-4i", out _));

        [TestMethod]
        public void ParseComplex7()
            => Assert.IsTrue(ComplexNumber.TryParse("-i", out _));

        public void ParseComplex8()
            => Assert.IsTrue(ComplexNumber.TryParse("-5.3i", out _));

        public void ParseComplex9()
            => Assert.IsTrue(ComplexNumber.TryParse("5.3", out _));
    }
}
