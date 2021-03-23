using AngouriMath;
using Xunit;
using static AngouriMath.Entity;

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

        [Fact] public void MatrixMultiplicationIdentity()
            => Assert.Equal(MathS.I_3, MathS.I_3 * MathS.I_3);

        [Fact] public void MatrixIdentityT()
            => Assert.Equal(Matrix.I(3), Matrix.I(3).T);

        [Fact] public void MatrixIdentityT2x3()
            => Assert.Equal(Matrix.I(2, 3), Matrix.I(3, 2).T);

        public static readonly Matrix H = MathS.Matrix(new Entity[,]
            {
                { 1, 1 },
                { 1, -1 },
            });

        [Fact] public void MatrixMultiplication1()
            => Assert.Equal(MathS.Vector(1, 1), H * MathS.Vector(0, 1));

        [Fact] public void MatrixMultiplication2()
            => Assert.Equal(MathS.I_2 + MathS.I_2, H * H);

        [Fact] public void MatrixAddition1()
            => Assert.Equal(MathS.Vector(3, 2), MathS.Vector(1, 2) + MathS.Vector(2, 0));

        [Fact] public void MatrixSubtraction1()
            => Assert.Equal(MathS.Vector(-1, 2), MathS.Vector(1, 2) - MathS.Vector(2, 0));

        [Fact] public void MatrixDivision1()
            => Assert.Equal(H, H / MathS.I_2);

        [Fact] public void MatrixDivision2()
            => Assert.Equal(
                MathS.Matrix(new Entity[,]{
                    { "1 / 2", "1 / 2" },
                    { "1 / 2", "-1 / 2" }
                }).InnerSimplified,
                MathS.I_2 / H);

        [Fact] public void MatrixDivision3()
            => Assert.Equal(MathS.I_2, H / H);

        [Fact] public void MatrixDivision3x3()
            => Assert.Equal(
                MathS.Matrix(new Entity[,] {
                        { 1, -3 },
                        { 3, -8 }
                    }),

                MathS.Matrix(new Entity[,] {
                        { 1, 0 },
                        { 3, 1 }
                    })
                /
                MathS.Matrix(new Entity[,] {
                        { 1, 3 },
                        { 0, 1 }
                    })

                );

        [Fact] public void LeftScalarMulitiplication()
            => Assert.Equal(MathS.Vector(6, 8), (2 * MathS.Vector(3, 4)).InnerSimplified);

        [Fact] public void RightScalarMulitiplication()
            => Assert.Equal(MathS.Vector(6, 8), (MathS.Vector(3, 4) * 2).InnerSimplified);

        [Fact] public void RightScalarDivision()
            => Assert.Equal(MathS.Vector("1.5", 2), (MathS.Vector(3, 4) / 2).InnerSimplified);

        [Fact] public void LeftScalarAddition()
            => Assert.Equal(MathS.Vector(5, 6), (2 + MathS.Vector(3, 4)).InnerSimplified);

        [Fact] public void RightScalarAddition()
            => Assert.Equal(MathS.Vector(5, 6), (MathS.Vector(3, 4) + 2).InnerSimplified);

        [Fact] public void LeftScalarSubtraction()
            => Assert.Equal(MathS.Vector(-3, -2), (2 - MathS.Vector(3, 4)).InnerSimplified);

        [Fact] public void RightScalarSubtraction()
            => Assert.Equal(MathS.Vector(1, 2), (MathS.Vector(3, 4) - 2).InnerSimplified);

        [Fact] public void Adjugate1()
        {
            var H = MathS.Matrix(new Entity[,]
            {
                { "1", "2", "6" },
                { "3", "2", "9" },
                { "1", "1", "9" },
            });
            var actual = (H.Adjugate / H.Determinant).Evaled;
            var expected = H.ComputeInverse();

            Assert.Equal(expected, actual);
        }

        [Fact] public void Adjugate2()
            => Assert.Equal(MathS.I_2, MathS.I_2.Adjugate);

        [Fact] public void GaussianElimination()
        {
            var m = MathS.Matrix(new Entity[,] {
                    { 32, 41, 1 },
                    { 3,  4,  1 },
                    { 3,  1,  4 }
                });
            var g = m.GaussianEliminated;
            Assert.Equal(m.Determinant, g.MainDiagonal[0] * g.MainDiagonal[1] * g.MainDiagonal[2]);
        }
    }
}
