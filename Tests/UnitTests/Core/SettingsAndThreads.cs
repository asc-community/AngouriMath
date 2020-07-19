using System.Threading;
using AngouriMath;
using AngouriMath.Core.Numerix;
using AngouriMath.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeterO.Numbers;

namespace UnitTests.Core
{
    [TestClass]
    public class SettingsAndThreads
    {
        [TestMethod]
        public void SettingValue1()
        {
            MathS.Settings.MaxExpansionTermCount.As(27, () =>
                {
                    Assert.AreEqual(27, MathS.Settings.MaxExpansionTermCount.Value);
                    MathS.Settings.MaxExpansionTermCount.As(134,
                        () =>
                        {
                            Assert.AreEqual(134, MathS.Settings.MaxExpansionTermCount.Value);
                        });
                    Assert.AreEqual(27, MathS.Settings.MaxExpansionTermCount.Value);
                }
            );
        }

        [TestMethod]
        public void SettingValue2()
        {
            MathS.Settings.PrecisionErrorCommon.As(27.4m, () =>
                {
                    Assert.AreEqual(27.4m, MathS.Settings.PrecisionErrorCommon.Value);
                    MathS.Settings.PrecisionErrorCommon.As(134.5m,
                        () =>
                        {
                            Assert.AreEqual(134.5m, MathS.Settings.PrecisionErrorCommon.Value);
                        });
                    Assert.AreEqual(27.4m, MathS.Settings.PrecisionErrorCommon.Value);
                }
            );
        }

        public void Solve(int num)
        {
            MathS.Settings.AllowNewton.As(num % 2 == 0, () =>
                {
                    var roots = "x2 + e^x + sin(x)".SolveEquation("x");
                    Assert.IsTrue((num % 2 == 0) == roots.Count > 0, 
                        $"Considering that allow Newton is {num % 2 == 0}, root count shouldn't be {roots.Count}");
                }
            );
        }

        [TestMethod]
        public void SettingThreadsSolve()
        {
            var th1 = new Thread(() => Solve(0));
            var th2 = new Thread(() => Solve(1));
            var th3 = new Thread(() => Solve(2));
            var th4 = new Thread(() => Solve(3));
            th1.Start();
            th2.Start();
            th3.Start();
            th4.Start();
            th1.Join();
            th2.Join();
            th3.Join();
            th4.Join();
        }

        public void Checker(int num)
        {
            MathS.Settings.MaxExpansionTermCount.As(num, () =>
            {
                for (int i = 0; i < 100000; i++)
                    Assert.AreEqual(num, MathS.Settings.MaxExpansionTermCount.Value);
            });
        }
        [TestMethod]
        public void SettingThreadsSeparateUse()
        {
            var threads = new Thread[3];
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(() => Checker(i));
                threads[i].Start();
            }
            for (int i = 0; i < threads.Length; i++)
                threads[i].Join();
        }
    }
}
