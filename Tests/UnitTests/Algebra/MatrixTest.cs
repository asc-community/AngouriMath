using AngouriMath;
using AngouriMath.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTests
{
    [TestClass]
    public class MatrixTest
    {
        public static readonly Tensor A = MathS.Matrix(2, 4,
            1, 2, 3, 4,
            5, 6, 7, 8
            );
        public static readonly Tensor B = MathS.Matrix(4, 2,
            1, 2,
            3, 4,
            5, 6,
            7, 8
            );
        [TestMethod]
        public void DotProduct1()
        {
            var C = MathS.Matrix(2, 2,
                50, 60,
                114, 140
                );
            Assert.IsTrue((A * B).EvalTensor() == C);
        }
    }
}
