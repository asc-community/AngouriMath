using AngouriMath;
using AngouriMath.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Algebra
{
    [TestClass]
    public class MatrixTest
    {
        public static readonly Tensor A = MathS.Matrix(4, 2,
            1, 2,
            3, 4,
            5, 6,
            7, 8
        );
        public static readonly Tensor B = MathS.Matrix(4, 2,
            1, 2,
            3, 4,
            5, 6,
            7, 8
            );
        public static readonly Tensor C = MathS.Matrix(2, 4,
            1, 2, 3, 4,
            5, 6, 7, 8
        );

        [TestMethod]
        public void DotProduct()
        {
            var R = MathS.Matrix(2, 2,
                50, 60,
                114, 140
            );
            Assert.IsTrue(MathS.DotProduct(C, B).EvalTensor() == R);
        }
        [TestMethod]
        public void DotPointwiseProduct1()
        {
            var C = MathS.Matrix(4, 2,
                1, 4,
                9, 16,
                25, 36,
                49, 64
                ); 
            Assert.IsTrue((A * B).EvalTensor() == C);
        }

        [TestMethod]
        public void SumProduct1()
        {
            Assert.IsTrue((A + B).EvalTensor() == (2 * A).EvalTensor());
        }

        [TestMethod]
        public void ScalarProduct1()
        {
            var a = MathS.Vector(1, 2, 3);
            var b = MathS.Vector(1, 2, 4);
            Assert.IsTrue(MathS.ScalarProduct(a, b).Eval() == 17);
        }
    }
}
