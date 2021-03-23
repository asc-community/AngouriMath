using AngouriMath;
using Xunit;

namespace UnitTests.Algebra
{
    public sealed class MatrixTest
    {
        public static readonly Entity.Matrix A = MathS.Matrices.Matrix(4, 2,
            1, 2,
            3, 4,
            5, 6,
            7, 8
        );
        public static readonly Entity.Matrix B = MathS.Matrices.Matrix(4, 2,
            1, 2,
            3, 4,
            5, 6,
            7, 8
            );
        public static readonly Entity.Matrix C = MathS.Matrices.Matrix(2, 4,
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
            Assert.Equal(R, A * B);
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
            Assert.Equal(C, A * B);
        }

        [Fact]
        public void SumProduct1() => Assert.Equal((2 * A).Evaled, (A + B).Evaled);

        [Fact]
        public void ScalarProduct1()
        {
            var a = MathS.Vector(1, 2, 3);
            var b = MathS.Vector(1, 2, 4);
            Assert.Equal(17, MathS.Matrices.ScalarProduct(a, b).EvalNumerical());
            Assert.Equal(17, (a.T * b).EvalNumerical());
        }

        [Fact]
        public void Determ()
        {
            var A = MathS.Matrix(new Entity[,]
            {
                {"A", "B"},
                {"C", "D"}
            });
            var v = A.Determinant.Substitute("A", 1)
                .Substitute("B", 2)
                .Substitute("C", 3)
                .Substitute("D", 4);
            Assert.Equal(-2, v.EvalNumerical());
        }

        [Fact]
        public void TransposeMatrix()
        {
            var a = MathS.Matrix(new Entity[,]
                {
                    { 1, 2 },
                    { 3, 4 }
                });
            Assert.Equal(1, a[0, 0]);
            Assert.Equal(2, a[0, 1]);
            Assert.Equal(3, a[1, 0]);
            Assert.Equal(4, a[1, 1]);

            Assert.Equal(1, a.T[0, 0]);
            Assert.Equal(3, a.T[0, 1]);
            Assert.Equal(2, a.T[1, 0]);
            Assert.Equal(4, a.T[1, 1]);
        }

        [Fact]
        public void TransposeVector()
        {
            var a = MathS.Vector(1, 2);
            Assert.Equal(1, a[0]);
            Assert.Equal(2, a[1]);

            Assert.Equal(1, a.T[0, 0]);
            Assert.Equal(2, a.T[0, 1]);
        }

        [Fact]
        public void TransposeImmutability()
        {
            var a = MathS.Vector(1, 2);
            var aT = MathS.Matrix(new Entity[,] { { 1, 2 } });
            Assert.Equal(aT, a.T);
            var origin = MathS.Vector(1, 2);
            Assert.Equal(origin, a);
        }

        [Fact]
        public void TransposeDoubleOperationVector()
        {
            var a = MathS.Vector(1, 2);
            Assert.Equal(a, a.T.T);
        }

        [Fact]
        public void TransposeDoubleOperationMatrix()
        {
            var a = MathS.Matrix(new Entity[,]
                {
                    { 1, 2 },
                    { 3, 4 }
                });
            Assert.Equal(a, a.T.T);
        }

        [Fact]
        public void TransposeDoubleOperationMatrixImmutability()
        {
            var a = MathS.Matrix(new Entity[,]
                {
                    { 1, 2 },
                    { 3, 4 }
                });
            var b = MathS.Matrix(new Entity[,]
                {
                    { 1, 2 },
                    { 3, 4 }
                });
            var c = a.T;
            Assert.Equal(b, c.T);
        }

        [Fact]
        public void Identity2()
        {
            Assert.Equal(
                MathS.Matrix(new Entity[,]
                {
                    { 1, 0 },
                    { 0, 1 }
                }),
                MathS.IdentityMatrix(2));
        }

        [Fact]
        public void Identity3()
        {
            Assert.Equal(
                MathS.Matrix(new Entity[,]
                {
                    { 1, 0, 0 },
                    { 0, 1, 0 },
                    { 0, 0, 1 }
                }),
                MathS.IdentityMatrix(3));
        }

        [Fact]
        public void Identity2x3()
        {
            Assert.Equal(
                MathS.Matrix(new Entity[,]
                {
                    { 1, 0, 0 },
                    { 0, 1, 0 },
                }),
                MathS.IdentityMatrix(2, 3));
        }

        [Fact]
        public void Identity3x2()
        {
            Assert.Equal(
                MathS.Matrix(new Entity[,]
                {
                    { 1, 0 },
                    { 0, 1 },
                    { 0, 0 }
                }),
                MathS.IdentityMatrix(3, 2));
        }

        [Fact]
        public void MatrixMultiplication()
        {
            
        }
    }
}
