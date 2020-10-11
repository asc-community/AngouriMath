using AngouriMath;
using Xunit;

namespace UnitTests.Algebra
{
    public class MatrixTest
    {
        public static readonly Entity.Tensor A = MathS.Matrices.Matrix(4, 2,
            1, 2,
            3, 4,
            5, 6,
            7, 8
        );
        public static readonly Entity.Tensor B = MathS.Matrices.Matrix(4, 2,
            1, 2,
            3, 4,
            5, 6,
            7, 8
            );
        public static readonly Entity.Tensor C = MathS.Matrices.Matrix(2, 4,
            1, 2, 3, 4,
            5, 6, 7, 8
        );

        [Fact]
        public void DotProduct()
        {
            var R = MathS.Matrices.Matrix(2, 2,
                50, 60,
                114, 140
            );
            Assert.Equal(R, MathS.Matrices.MatrixMultiplication(C, B).EvalTensor());
        }
        [Fact]
        public void DotPointwiseProduct1()
        {
            var C = MathS.Matrices.Matrix(4, 2,
                1, 4,
                9, 16,
                25, 36,
                49, 64
                ); 
            Assert.Equal(C, (A * B).EvalTensor());
        }

        [Fact]
        public void SumProduct1() => Assert.Equal((2 * A).EvalTensor(), (A + B).EvalTensor());

        [Fact]
        public void ScalarProduct1()
        {
            var a = MathS.Matrices.Vector(1, 2, 3);
            var b = MathS.Matrices.Vector(1, 2, 4);
            Assert.Equal(17, MathS.Matrices.ScalarProduct(a, b).EvalNumerical());
        }

        [Fact]
        public void Determ()
        {
            var A = MathS.Matrices.Matrix(new Entity[,]
            {
                {"A", "B"},
                {"C", "D"}
            });
            var v = A.Determinant().Substitute("A", 1)
                .Substitute("B", 2)
                .Substitute("C", 3)
                .Substitute("D", 4);
            Assert.Equal(-2, v.EvalNumerical());
        }
    }
}
