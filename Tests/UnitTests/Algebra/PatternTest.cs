using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Core.TreeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Text;

namespace UnitTests.Algebra
{
    [TestClass]
    public class PatternTest
    {
        [TestMethod]
        public void TestEnumerator1()
        {
            Entity expr = "a^x + b^y + c^z";
            var pattern = Powf.PHang(Patterns.any1, Patterns.any2);

            var matches = new Set();
            foreach(var match in TreeAnalyzer.GetPatternEnumerator(expr, pattern))
            {
                matches.Add(match);
            }
            Assert.AreEqual(3, matches.Count);
            foreach(var match in matches.FiniteSet())
            {
                Assert.IsNotNull(expr.FindSubtree(match), "match is not in expression");
            }
        }

        [TestMethod]
        public void TestEnumerator2()
        {
            Entity expr = "a^x + ((b^y + c^z)^2)^x";
            var pattern = Powf.PHang(Patterns.any1, Patterns.any2);

            var matches = new Set();
            foreach (var match in TreeAnalyzer.GetPatternEnumerator(expr, pattern))
            {
                matches.Add(match);
            }
            Assert.AreEqual(5, matches.Count);
            foreach (var match in matches.FiniteSet())
            {
                Assert.IsNotNull(expr.FindSubtree(match), "match is not in expression");
            }
        }

        [TestMethod]
        public void TestEnumerator3()
        {
            Entity expr = "2 + 3 + x";
            var pattern = Powf.PHang(Patterns.any1, Patterns.any2);

            var matches = new Set();
            foreach (var match in TreeAnalyzer.GetPatternEnumerator(expr, pattern))
            {
                matches.Add(match);
            }
            Assert.AreEqual(0, matches.Count);
        }
    }
}
