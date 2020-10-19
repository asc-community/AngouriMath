using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Core.Exceptions;
using System.Linq;
using UnitTests.Algebra;
using Xunit;
using static AngouriMath.Entity.Number;
using static AngouriMath.Entity.Set;
using static AngouriMath.MathS;
using AngouriMath.Extensions;
using System.Linq.Expressions;
using System;

namespace UnitTests.Convenience
{
    public class ExtensionTest
    {
        [Fact]
        public void TestSystem2()
        {
            var res = ("x", "y").SolveSystem("x", "y");
            var exp = MathS.Matrices.Matrix(new Entity[,] { { 0, 0 } });
            Assert.Equal(exp, res);
        }

        [Fact]
        public void TestSystem3()
        {
            var res = ("x", "y", "z").SolveSystem("x", "y", "z");
            var exp = MathS.Matrices.Matrix(new Entity[,] { { 0, 0, 0 } });
            Assert.Equal(exp, res);
        }

        [Fact]
        public void TestSystem4()
        {
            var res = ("x", "y", "z", "t").SolveSystem("x", "y", "z", "t");
            var exp = MathS.Matrices.Matrix(new Entity[,] { { 0, 0, 0, 0 } });
            Assert.Equal(exp, res);
        }

        [Fact]
        public void TestSystem5()
        {
            var res = ("x", "y", "z", "t", "k").SolveSystem("x", "y", "z", "t", "k");
            var exp = MathS.Matrices.Matrix(new Entity[,] { { 0, 0, 0, 0, 0 } });
            Assert.Equal(exp, res);
        }

        [Fact]
        public void TestSystem6()
        {
            var res = ("x", "y", "z", "t", "k", "p").SolveSystem("x", "y", "z", "t", "k", "p");
            var exp = MathS.Matrices.Matrix(new Entity[,] { { 0, 0, 0, 0, 0, 0 } });
            Assert.Equal(exp, res);
        }

        [Fact]
        public void TestSystem7()
        {
            var res = ("x", "y", "z", "t", "k", "p", "r").SolveSystem("x", "y", "z", "t", "k", "p", "r");
            var exp = MathS.Matrices.Matrix(new Entity[,] { { 0, 0, 0, 0, 0, 0, 0 } });
            Assert.Equal(exp, res);
        }

        [Fact]
        public void TestSystem8()
        {
            var res = ("x", "y", "z", "t", "k", "p", "r", "l").SolveSystem("x", "y", "z", "t", "k", "p", "r", "l");
            var exp = MathS.Matrices.Matrix(new Entity[,] { { 0, 0, 0, 0, 0, 0, 0, 0 } });
            Assert.Equal(exp, res);
        }
    }
}
