using System;
using AngouriMath;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AngouriMath.Core.TreeAnalysis;
using AngouriMath.Core.Exceptions;
using AngouriMath.Core.FromString;

namespace UnitTests.Common
{
    [TestClass]
    public class ExceptionTest
    {
        [TestMethod]
        public void TreeCheck1()
            => Assert.ThrowsException<TreeException>(() =>
            {
                Entity expr = "x2 + x3 / 0";
                MathS.Utils.CheckTree(expr);
            }, "division by zero");

        [TestMethod]
        public void TreeCheck2()
            => Assert.ThrowsException<TreeException>(() =>
            {
                Entity expr = "x2 + x3 / 0";
                expr.AddChild(expr);
                MathS.Utils.CheckTree(expr);
            }, "division by zero");

        [TestMethod]
        public void NotImplemented()
        {
            var p = new Pattern(3, Entity.PatType.COMMON, Const.Patterns.AlwaysTrue);
            Assert.ThrowsException<NoNeedToImplementException>(p.Check, "No need to implement");
            Assert.ThrowsException<NoNeedToImplementException>(() => p == p + 1, "No need to implement");
            Assert.ThrowsException<NoNeedToImplementException>(() => p.Simplify(), "No need to implement");
        }

        [TestMethod]
        public void WrongNumbersOfArgs1()
            => Assert.ThrowsException<ParseException>(() => (Entity) "log(3)");

        [TestMethod]
        public void WrongNumbersOfArgs2()
            => Assert.ThrowsException<ParseException>(() => (Entity) "ln(3, 5)");

        [TestMethod]
        public void WrongNumbersOfArgs3()
            => Assert.ThrowsException<ParseException>(() => (Entity) "sin(3, 5, 8)");

    }
}