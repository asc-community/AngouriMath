using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Functions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Common
{
    [TestClass]
    public class NonStrictTest
    {
        static readonly Entity.Var x = nameof(x);
        [TestMethod]
        public void TensorLatex()
        {
            var tens = MathS.Matrices.Matrix(2, 2, 1342, 2123, 1423, 1122);
            Assert.IsTrue(tens.Latexise().Length > 16);
        }

        [TestMethod]
        public void TensorFull()
        {
            var tens = new Entity.Tensor(3, 4, 5);
            for (int i = 0; i < 3; i++)
            for (int j = 0; j < 4; j++)
            for (int k = 0; k < 5; k++)
                tens[i, j, k] = i * j * k; 
            Assert.IsTrue(tens.ToString().Length > 16);
        }

        [TestMethod]
        public void EqSysLatex()
        {
            var eq = MathS.Equations(
                "x + 3",
                "y + x + 5"
            );
            Assert.IsTrue(eq.Latexise().Length > 10);
        }

        [TestMethod]
        public void SympySyntax()
        {
            Entity expr = "x + 4 + e";
            Assert.IsTrue(MathS.Utils.ToSympyCode(expr).Length > 10);
        }

        [TestMethod]
        public void TryPoly1()
        {
            Entity expr = "x + x2";
            if (Utils.TryPolynomial(expr, x, out var dst))
                Assert.AreEqual(MathS.FromString("x2 + x"), dst);
            else
                Assert.Fail(expr.ToString() + " is polynomial");
        }

        [TestMethod]
        public void TryPoly2()
        {
            Entity expr = "x * (x + x2)";
            if (Utils.TryPolynomial(expr, x, out var dst))
                Assert.AreEqual(MathS.FromString("x3 + x2"), dst);
            else
                Assert.Fail(expr.ToString() + " is polynomial");
        }

        [TestMethod]
        public void TryPoly3()
        {
            Entity expr = "x * (x + x2 + z) + y * x";
            if (Utils.TryPolynomial(expr, x, out var dst))
                Assert.AreEqual(MathS.FromString("x3 + x2 + (y + z) * x"), dst);
            else
                Assert.Fail(expr.ToString() + " is polynomial");
        }
    }
}
