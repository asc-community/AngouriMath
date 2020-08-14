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
        public void WrongNumbersOfArgs0()
            => Assert.ThrowsException<FunctionArgumentCountException>(() => (Entity) "limitleft()",
                "limitleft should have exactly 3 arguments but 0 arguments are provided");

        [TestMethod]
        public void WrongNumbersOfArgs1()
            => Assert.ThrowsException<FunctionArgumentCountException>(() => (Entity)"derivative(3)",
                "derivative should have exactly 3 arguments but 1 argument is provided");

        [TestMethod]
        public void WrongNumbersOfArgs2()
            => Assert.ThrowsException<FunctionArgumentCountException>(() => (Entity) "ln(3, 5)",
                "ln should have exactly 1 argument but 2 arguments are provided");

        [TestMethod]
        public void WrongNumbersOfArgs3()
            => Assert.ThrowsException<FunctionArgumentCountException>(() => (Entity) "sin(3, 5, 8)",
                "sin should have exactly 1 argument but 3 arguments are provided");

        [TestMethod]
        public void WrongNumbersOfArgsLog()
            => Assert.ThrowsException<FunctionArgumentCountException>(() => (Entity) "log()",
                "log should have 1 argument or 2 arguments but 0 arguments are provided");

    }
}