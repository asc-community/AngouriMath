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