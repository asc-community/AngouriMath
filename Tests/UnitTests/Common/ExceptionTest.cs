using System;
using AngouriMath;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AngouriMath.Core.Exceptions;

namespace UnitTests.Common
{
    [TestClass]
    public class ExceptionTest
    {
        public void AssertExceptionThrown(Func<bool> func, string? msg = null)
        {
            try
            {
                func();
                Assert.Fail("An exception should have been occured");
            }
            catch (SysException e)
            {
                if (msg is { })
                    Assert.AreEqual(msg, e.Message);
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
                MathS.Utils.CheckTree(expr);
                return true;
            });

        [TestMethod]
        public void TreeCheck2()
            => AssertExceptionThrown(() =>
            {
                Entity expr = "x2 + x3 / 0";
                expr.Children.Add(expr);
                MathS.Utils.CheckTree(expr);
                return true;
            });

        [TestMethod]
        public void NotImplemented()
        {
            var p = new Pattern(3, Entity.PatType.COMMON, Const.Patterns.AlwaysTrue);
            AssertExceptionThrown((() =>
            {
                p.Check();
                return true;
            }), "@");
            AssertExceptionThrown((() =>
            {
                var _ = p == p + 1;
                return true;
            }), "@");
            AssertExceptionThrown((() =>
            {
                p.Simplify();
                return true;
            }), "@");
        }
    }
}