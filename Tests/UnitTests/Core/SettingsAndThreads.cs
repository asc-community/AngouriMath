using System;
using System.Threading;
using AngouriMath;
using AngouriMath.Core.Numerix;
using AngouriMath.Extensions;
using Xunit;
using PeterO.Numbers;

namespace UnitTests.Core
{
    public class SettingsAndThreads
    {
        [Fact]
        public void SettingValue1()
        {
            MathS.Settings.MaxExpansionTermCount.As(27, () =>
                {
                    Assert.Equal(27, MathS.Settings.MaxExpansionTermCount.Value);
                    MathS.Settings.MaxExpansionTermCount.As(134,
                        () =>
                        {
                            Assert.Equal(134, MathS.Settings.MaxExpansionTermCount.Value);
                        });
                    Assert.Equal(27, MathS.Settings.MaxExpansionTermCount.Value);
                }
            );
        }

        [Fact]
        public void SettingValue2()
        {
            MathS.Settings.PrecisionErrorCommon.As(27.4m, () =>
                {
                    Assert.Equal(27.4m, MathS.Settings.PrecisionErrorCommon.Value);
                    MathS.Settings.PrecisionErrorCommon.As(134.5m,
                        () =>
                        {
                            Assert.Equal(134.5m, MathS.Settings.PrecisionErrorCommon.Value);
                        });
                    Assert.Equal(27.4m, MathS.Settings.PrecisionErrorCommon.Value);
                }
            );
        }

        [Fact]
        public void SettingThreadsSolve()
        {
            static void Solve(int num)
            {
                MathS.Settings.AllowNewton.As(num % 2 == 0, () =>
                {
                    var roots = "x2 + e^x + sin(x)".SolveEquation("x");
                    Assert.True((num % 2 == 0) == roots.Count > 0,
                        $"Considering that allow Newton is {num % 2 == 0}, root count shouldn't be {roots.Count}");
                });
            }
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

        [Fact]
        public void SettingThreadsSeparateUse()
        {
            static void Checker(int num)
            {
                MathS.Settings.MaxExpansionTermCount.As(num, () =>
                {
                    for (int i = 0; i < 100000; i++)
                        Assert.Equal(num, MathS.Settings.MaxExpansionTermCount.Value);
                });
            }
            var threads = new Thread[3];
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(() => Checker(i));
                threads[i].Start();
            }
            for (int i = 0; i < threads.Length; i++)
                threads[i].Join();
        }

        [Fact]
        public void WithExceptionInside()
        {
            MathS.Settings.MaxExpansionTermCount.As(500, () =>
                {
                    Assert.Equal(500, MathS.Settings.MaxExpansionTermCount.Value);
                    Assert.Throws<ArgumentException>(() =>
                        MathS.Settings.MaxExpansionTermCount.As(300, () =>
                            // something happens
                            throw new ArgumentNullException() // some random exception
                        ));
                    // should be kept as 500
                    Assert.Equal(500, MathS.Settings.MaxExpansionTermCount.Value);
                }
            );
        }
    }
}
