using System;
using AngouriMath;
using AngouriMath.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AngouriMath.Core.Exceptions;

namespace UnitTests.Common
{
    [TestClass]
    public class ExceptionTest
    {
        public void AssertExceptionThrown(Func<bool> func)
        {
            try
            {
                func();
            }
            catch (SysException e)
            {
                // We don't do anything as it's ok
            }
            catch (Exception e)
            {
                Assert.Fail("Unexpected exception: " + e.Message);
            }
        }

        [TestMethod]
        public void TreeCheck1()
            => AssertExceptionThrown(() =>
            {
                Entity expr = "x2 + x3 / 0";
                MathS.CheckTree(expr);
                return true;
            });

        [TestMethod]
        public void TreeCheck2()
            => AssertExceptionThrown(() =>
            {
                Entity expr = "x2 + x3 / 0";
                expr.Children.Add(expr);
                MathS.CheckTree(expr);
                return true;
            });
    }
}