﻿//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using Xunit;
using static AngouriMath.Entity;
using AngouriMath.Extensions;

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
                84, 100,
                100, 120
            );
            Assert.Equal(R, A.T * B);
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
            Assert.Equal(C, MathS.Matrices.PointwiseMultiplication(A, B));
        }

        [Fact]
        public void SumProduct1() => Assert.Equal((2 * A).Evaled, (A + B).Evaled);

        [Fact]
        public void ScalarProduct1()
        {
            var a = MathS.Vector(1, 2, 3);
            var b = MathS.Vector(1, 2, 4);
            var scalar = a.T * b;
            Assert.Equal(17, scalar.EvalNumerical());
        }

        [Fact]
        public void Determ()
        {
            var A = MathS.Matrix(new Entity[,]
            {
                {"A", "B"},
                {"C", "D"}
            });
            var v = TestExtensions.AsNotNull(A.Determinant)
                .Substitute("A", 1)
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
            => Assert.Equal(MathS.Vector(1, -1), H * MathS.Vector(0, 1));

        [Fact] public void MatrixMultiplication2()
            => Assert.Equal(MathS.I_2 + MathS.I_2, H * H);

        [Fact] public void UnsuccessfulMultiplicationEvaled()
            => Assert.Equal(A * (Entity)B, (A * (Entity)B).Evaled);

        [Fact] public void UnsuccessfulMultiplicationInnerSimplified()
            => Assert.Equal(A * (Entity)B, (A * (Entity)B).InnerSimplified);

        [Fact] public void MatrixAddition1()
            => Assert.Equal(MathS.Vector(3, 2), MathS.Vector(1, 2) + MathS.Vector(2, 0));

        [Fact] public void UnsuccessfulMatrixAddition2Evaled()
            => Assert.Equal(MathS.Vector(1, 2) + (Entity)MathS.Vector(2, 0, 3), (MathS.Vector(1, 2) + (Entity)MathS.Vector(2, 0, 3)).Evaled);

        [Fact] public void UnsuccessfulMatrixAddition2InnerSimplified()
            => Assert.Equal(MathS.Vector(1, 2) + (Entity)MathS.Vector(2, 0, 3), (MathS.Vector(1, 2) + (Entity)MathS.Vector(2, 0, 3)).InnerSimplified);

        [Fact] public void MatrixSubtraction1()
            => Assert.Equal(MathS.Vector(-1, 2), MathS.Vector(1, 2) - MathS.Vector(2, 0));

        [Fact] public void UnsuccessfulMatrixSubtraction2Evaled()
            => Assert.Equal(MathS.Vector(1, 2) - (Entity)MathS.Vector(2, 0, 3), (MathS.Vector(1, 2) - (Entity)MathS.Vector(2, 0, 3)).Evaled);

        [Fact] public void UnsuccessfulMatrixSubtraction2InnerSimplified()
            => Assert.Equal(MathS.Vector(1, 2) - (Entity)MathS.Vector(2, 0, 3), (MathS.Vector(1, 2) - (Entity)MathS.Vector(2, 0, 3)).InnerSimplified);

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
            => Assert.Equal(MathS.Vector(-1, -2), (2 - MathS.Vector(3, 4)).InnerSimplified);

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
            var actual = H.Adjugate is { } adj && H.Determinant is { } det ? (adj / det).Evaled : null;
            var expected = H.Inverse;

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
            var g = m.RowEchelonForm;
            Assert.Equal(m.Determinant, (g.MainDiagonal[0] * g.MainDiagonal[1] * g.MainDiagonal[2]).InnerSimplified);
        }

        [Fact] public void ZeroSquareMatrix1()
            => Assert.Equal(MathS.O_3, (MathS.I_3 * 0).InnerSimplified);

        [Fact] public void ZeroSquareMatrix2()
            => Assert.Equal(MathS.O_4, (MathS.I_4 * 0).InnerSimplified);

        [Fact] public void ZeroVector()
            => Assert.Equal(MathS.Vector(0, 0, 0, 0), MathS.ZeroVector(4));

        [Fact] public void ZeroRectMatrix()
        {
            var m = MathS.Matrix(new Entity[,]{
                { 0, 0, 0 },
                { 0, 0, 0 }
            });
            Assert.Equal(m, MathS.ZeroMatrix(2, 3));
        }

        [Fact] public void WithElementVector1()
        {
            var v = MathS.Vector(1, 2, 3);
            Assert.Equal(5, v.WithElement(2, 5)[2]);
            Assert.Equal(MathS.Vector(1, 2, 3), v);
        }
        [Fact] public void WithElementVector2()
        {
            var v = MathS.ZeroVector(5);
            Assert.Equal(MathS.Vector(0, 0, 5, 6, 0), v.WithElement(2, 5).WithElement(3, 6));
            Assert.Equal(MathS.Vector(0, 0, 0, 0, 0), v);
        }

        [Fact] public void WithElementMatrix1()
        {
            var jordan = MathS.I_3.WithElement(0, 1, 1).WithElement(1, 2, 1);
            Assert.Equal(
                MathS.Matrix(new Entity[,]
                {
                    { 1, 1, 0 },
                    { 0, 1, 1 },
                    { 0, 0, 1 }
                }),
                jordan);
        }

        [Fact] public void WithElementMatrix2()
        {
            var jordan = MathS.I_3.WithElement(0, 1, 5).WithElement(1, 2, 8);
            Assert.Equal(
                MathS.Matrix(new Entity[,]
                {
                    { 1, 5, 0 },
                    { 0, 1, 8 },
                    { 0, 0, 1 }
                }),
                jordan);
        }

        [Fact] public void WithRowVector()
            => Assert.Equal(MathS.Vector(0, 0, 3, 0), MathS.ZeroVector(4).WithRow(2, MathS.Vector(3)));

        [Fact] public void WithRowMatrix1()
        {
            var m = MathS.I_4.WithRow(2, MathS.Vector(1, 2, 3, 4).T);
            Assert.Equal(
                MathS.Matrix(new Entity[,]
                {
                    { 1, 0, 0, 0 },
                    { 0, 1, 0, 0 },
                    { 1, 2, 3, 4 },
                    { 0, 0, 0, 1 }
                }),
                m);
        }

        [Fact] public void WithRowMatrix2()
        {
            var m = MathS.IdentityMatrix(5)
                .WithRow(2, MathS.Vector(1, 2, 3, 4, 5).T)
                .WithRow(4, MathS.Vector(1, 3, 5, 9, 1).T)
                .WithRow(4, MathS.Vector(5, 6, 7, 8, 9).T);
            Assert.Equal(
                MathS.Matrix(new Entity[,]
                {
                    { 1, 0, 0, 0, 0 },
                    { 0, 1, 0, 0, 0 },
                    { 1, 2, 3, 4, 5 },
                    { 0, 0, 0, 1, 0 },
                    { 5, 6, 7, 8, 9 }
                }),
                m);
        }

        [Fact] public void WithColumnVector()
            => Assert.Equal(MathS.Vector(1, 2, 3), MathS.ZeroVector(3).WithColumn(0, MathS.Vector(1, 2, 3)));

        [Fact] public void WithColumnMatrix1()
        {
            var m = MathS.I_4.WithColumn(2, MathS.Vector(1, 2, 3, 4));
            Assert.Equal(
                MathS.Matrix(new Entity[,]
                {
                    { 1, 0, 1, 0 },
                    { 0, 1, 2, 0 },
                    { 0, 0, 3, 0 },
                    { 0, 0, 4, 1 }
                }),
                m);
        }

        [Fact] public void WithColumnMatrix2()
        {
            var m = MathS.IdentityMatrix(5)
                .WithColumn(2, MathS.Vector(1, 2, 3, 4, 5))
                .WithColumn(4, MathS.Vector(1, 3, 5, 9, 1))
                .WithColumn(4, MathS.Vector(5, 6, 7, 8, 9));
            Assert.Equal(
                MathS.Matrix(new Entity[,]
                {
                    { 1, 0, 1, 0, 5 },
                    { 0, 1, 2, 0, 6 },
                    { 0, 0, 3, 0, 7 },
                    { 0, 0, 4, 1, 8 },
                    { 0, 0, 5, 0, 9 }
                }),
                m);
        }

        [Fact] public void WithMatrix1()
        {
            var m = MathS.ZeroMatrix(6).With
                ((r, c, e) => (r, c, e) switch
                {
                    (_, < 1, _) => 1,
                    (>= 1 and <= 4, >= 1 and <= 3, _) => 2,
                    (>= 1 and <= 4, > 3, _) => 3,
                    (5, 5, _) => 4,
                    _ => e
                });
            Assert.Equal(
                MathS.Matrix(new Entity[,]
                {
                    { 1, 0, 0, 0, 0, 0 },
                    { 1, 2, 2, 2, 3, 3 },
                    { 1, 2, 2, 2, 3, 3 },
                    { 1, 2, 2, 2, 3, 3 },
                    { 1, 2, 2, 2, 3, 3 },
                    { 1, 0, 0, 0, 0, 4 },
                }),
                m);
        }

        [Fact] public void ReducedRowEchelonForm1()
        {
            var m2 = MathS.Matrix(new Entity[,]
                {
                    {  1,  -1,  0, -2,  0 },
                    { -3,  -6, -2,  1,  3 },
                    {  0,  -7, -1, -5,  2 },
                    {  3,   4,  1, -1, -2 },
                    { -6, -19, -5, -3,  8 }
                }
            );
            Assert.Equal(
                MathS.Matrix(new Entity[,]
                {
                    { 1, 0, 0, -1, "-1/5".Simplify() },
                    { 0, 1, 0,  1, "-1/5".Simplify() },
                    { 0, 0, 1, -2, "-3/5".Simplify() },
                    { 0, 0, 0,  0, 0 },
                    { 0, 0, 0,  0, 0 },
                }),
                m2.ReducedRowEchelonForm);
        }

        [Fact] public void ReducedRowEchelonForm2()
        {
            var m2 = MathS.Matrix(new Entity[,]
                {
                    { 1,  3,  1,  9 },
                    { 1,  1, -1,  1 },
                    { 3, 11,  5, 25 },
                    { 5, 6,  78, 23 }
                }
            );
            Assert.Equal(
                MathS.Matrix(new Entity[,]
                {
                    { 1,  0,  0,  0 },
                    { 0,  1,  0,  0 },
                    { 0,  0,  1,  0 },
                    { 0,  0,  0,  1 }
                }),
                m2.ReducedRowEchelonForm);
        }

        [Fact] public void RowEchelonForm1()
        {   
            var m2 = MathS.Matrix(new Entity[,]
                {
                    { 1,  3,  0,  9 },
                    { 0,  0,  0,  1 },
                    { 3, 11,  0, 25 },
                }
            );
            Assert.Equal(
                MathS.Matrix(new Entity[,]
                {
                    { 1, 3, 0,  9 },
                    { 0, 2, 0, -2 },
                    { 0, 0, 0,  1 }
                }),
                m2.RowEchelonForm);
        }

        [Fact] public void RowEchelonForm2()
        {   
            var m2 = MathS.Matrix(new Entity[,]
                {
                    { 8,  3 },
                    { 9,  1 },
                    { 3,  7 },
                    { 5,  6 }
                }
            );
            Assert.Equal(
                MathS.Matrix(new Entity[,]
                {
                    { 8,  3 },
                    { 0, "-19/8".Simplify() },
                    { 0,  0 },
                    { 0,  0 }
                }),
                m2.RowEchelonForm);
        }

        [Fact] public void Rank1()
        {
            var m2 = MathS.Matrix(new Entity[,]
                {
                    {  1,  -1,  0, -2,  0 },
                    { -3,  -6, -2,  1,  3 },
                    {  0,  -7, -1, -5,  2 },
                    {  3,   4,  1, -1, -2 },
                    { -6, -19, -5, -3,  8 }
                }
            );
            Assert.Equal(3, m2.Rank);
        }

        [Fact] public void Rank2()
        {
            Assert.Equal(6, MathS.IdentityMatrix(6).Rank);
        }

        [Theory]
        [InlineData("[a, b]", "[c, d]", "[a c, a d, b c, b d]")]
        [InlineData("[[a, b], [c, d]]", "[[e, f], [g, h]]", "[ [a e, a f, b e, b f], [a g, a h, b g, b h], [c e, c f, d e, d f], [c g, c h, d g, d h] ]")]
        [InlineData("[a, b]", "[c, d, e]", "[a c, a d, a e, b c, b d, b e]")]
        [InlineData("[a, b, c]", "[d, e]", "[a d, a e, b d, b e, c d, c e]")]
        [InlineData("[a, b, c]T", "[d, e]", "[[a d, a e], [b d, b e], [c d, c e]]T")]
        public void TensorProductTest(string a, string b, string expectedRaw)
        {
            Matrix A = a;
            Matrix B = b;
            Matrix expected = expectedRaw;
            var actual = Matrix.TensorProduct(A, B);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("[2]", 6, "[64]")]
        [InlineData("[2, 3]", 2, "[4, 6, 6, 9]")]
        [InlineData("[a, b]", 2, "[a 2, a b, b a, b 2]")]
        [InlineData("[[a, b], [c, d]]", 2, "[[a 2, a b, b a, b 2], [a c, a d, b c, b d], [c a, c b, d a, d b], [c 2, c d, d c, d 2]]")]
        [InlineData("[a, b]T", 2, "[a 2, a b, b a, b 2]T")]
        [InlineData("[a, b, c]T", 2, "[a 2, a b, a c, b a, b 2, b c, c a, c b, c 2]T")]
        [InlineData("[[a, b], [c, d], [e, f]]T", 2, "[[a 2, a b, b a, b 2], [a c, a d, b c, b d], [a e, a f, b e, b f], [c a, c b, d a, d b], [c 2, c d, d c, d 2], [c e, c f, d e, d f], [e a, e b, f a, f b], [e c, e d, f c, f d], [e 2, e f, f e, f 2]]T")]
        public void TensorPowerTest(string baseRaw, int power, string expectedRaw)
        {
            Matrix @base = baseRaw;
            var actual = @base.TensorPower(power).InnerSimplified;
            Matrix expected = expectedRaw;

            actual.ShouldBe(expected.InnerSimplified);
        }

        [Theory]
        [InlineData(1, 1, 2)]
        [InlineData(1, 2, 2)]
        [InlineData(2, 1, 2)]
        [InlineData(3, 4, 2)]
        [InlineData(3, 4, 3)]
        [InlineData(3, 4, 4)]
        [InlineData(1, 2, 3)]
        [InlineData(2, 1, 3)]
        [InlineData(1, 2, 4)]
        [InlineData(2, 1, 4)]
        [InlineData(1, 2, 5)]
        [InlineData(2, 1, 5)]
        [InlineData(1, 2, 6)]
        [InlineData(2, 1, 6)]
        [InlineData(2, 3, 5)]
        public void TensorPowerSizeTest(int rowCount, int columnCount, int n)
        {
            var m = MathS.ZeroMatrix(rowCount, columnCount);
            var actual = m.TensorPower(n);
            Assert.Equal((int)System.Math.Pow(rowCount, n), actual.RowCount);
            Assert.Equal((int)System.Math.Pow(columnCount, n), actual.ColumnCount);
        }

        [Theory]
        [InlineData("[[1, 0, 70], [0, 1, 0], [0, 0, 1]]")]
        [InlineData("[[1, 70, 0], [0, 1, 0], [0, 0, 1]]")]
        [InlineData("[[1, 70], [0, 1]]")]
        [InlineData("[[1, 0, 0, 0], [0, 1, 70, 0], [0, 0, 1, 0], [0, 0, 0, 1]]")]
        [InlineData("[6]")]
        public void TestInverseWithGT101Issue400(string matrixRaw)
        {
            Matrix a = matrixRaw;
            var inverse = TestExtensions.AsNotNull(a.Inverse);
            var product = (a * inverse).Simplify();
            if (a.RowCount == 1)
                Assert.Equal(1, product);
            else
                Assert.Equal(MathS.IdentityMatrix(a.RowCount), product);
        }

        [Theory]
        [InlineData("[1, 2, 3]")]
        [InlineData("[1, 2, 3]T")]
        [InlineData("[[1, 3], [2, 5], [3, 6]]")]
        public void CasesWhenDeterminantIsNull(string matrixRaw)
        {
            Matrix m = matrixRaw;
            Assert.Null(m.Determinant);
        }

        [Theory]
        [InlineData("[1, 2, 3]")]
        [InlineData("[1, 2, 3]T")]
        [InlineData("[[1, 3], [2, 5], [3, 6]]")]
        [InlineData("[[0, 0, 0], [3, 4, 5], [6, 7, 81]]")]
        public void CasesWhenInverseIsNull(string matrixRaw)
        {
            Matrix m = matrixRaw;
            Assert.Null(m.Inverse);
        }

        [Fact] public void TestMatrixMapped1()
            => Assert.Equal(MathS.I_3, MathS.Matrix(3, 3, (a, b) => a == b ? 1 : 0));

        [Fact] public void TestMatrixMapped2()
            => Assert.Equal("[[11, 12, 13], [21, 22, 23], [31, 32, 33]]", MathS.Matrix(3, 3, (a, b) => (a + 1) * 10 + (b + 1)));

        [Fact] public void TestMatrixMapped3()
            => Assert.Equal("[1616]", MathS.Matrix(1, 1, (a, b) => 1616));

        [Fact] public void TestMatrixMapped4()
            => Assert.Equal("[[1, 2], [3, 4], [5, 6]]", MathS.Matrix(3, 2, (a, b) => 1 + a * 2 + b));
    }
}
