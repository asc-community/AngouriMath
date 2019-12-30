using AngouriMath;
using AngouriMath.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq.Expressions;

namespace UnitTests
{
    [TestClass]
    public class FromLinqTest
    {
        public readonly static VariableEntity x = MathS.Var("x");
        [TestMethod]
        public void Test1()
        {
            Expression<Func<double, double>> lambda = x => Math.Sqrt(2) * Math.Sin(x);
            Assert.IsTrue(MathS.FromLinq(lambda) == MathS.Sqrt(2) * MathS.Sin(x));
        }
        [TestMethod]
        public void Test2()
        {
            Expression<Func<double, double>> lambda = x => Math.Pow(2, 3) * Math.Tan(x);
            var expected = MathS.Pow(2, 3) * MathS.Tan(x);
            var actual = MathS.FromLinq(lambda);
            Assert.IsTrue(expected == actual);
        }
        [TestMethod]
        public void Test3()
        {
            Expression<Func<double, double>> lambda = x => x;
            Assert.IsTrue(MathS.FromLinq(lambda) == x);
        }
        [TestMethod]
        public void Test4()
        {
            Expression<Func<double, double>> lambda = x => Math.Log(x, x * 3);
            Assert.IsTrue(MathS.FromLinq(lambda) == MathS.Log(x, x * 3));
        }
    }
}
