using System;
using AngouriMath;
using AngouriMath.Extensions;
using Xunit;
using static AngouriMath.Entity;

namespace UnitTests.Core.Multithreading
{
    public sealed class SettingsAndThreads
    {
        [Fact]
        public void SettingValue1()
        {
            MathS.Settings.MaxExpansionTermCount.As(27, () =>
                {
                    Assert.Equal(27, MathS.Settings.MaxExpansionTermCount.Value);
                    MathS.Settings.MaxExpansionTermCount.As(134,
                        () => Assert.Equal(134, MathS.Settings.MaxExpansionTermCount.Value));
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
                        () => Assert.Equal(134.5m, MathS.Settings.PrecisionErrorCommon.Value));
                    Assert.Equal(27.4m, MathS.Settings.PrecisionErrorCommon.Value);
                }
            );
        }

        // NOTE: Use Tasks instead of Threads because with Threads, unhandled exceptions
        // especially from Assertions crash the test runner entirely as exceptions on separate threads
        // are not handled by the main test thread, leading to unran tests. Using Tasks instead
        // ensures exceptions in worker threads are handled by the main test thread.
        [Fact]
        public void SettingParallelSolve()
        {
            static void Solve(int num) =>
                MathS.Settings.AllowNewton.As(num % 2 == 0, () =>
                {
                    var roots = "x2 + e^x + sin(x)".SolveEquation("x");
                    var set = (Set)roots.Simplify();
                    Assert.Equal(num % 2 == 0, !set.IsSetEmpty);
                });
            new ThreadingChecker(Solve).Run();
        }

        [Fact]
        public void SettingParallelSeparateUse()
        {
            static void Checker(int num) =>
                MathS.Settings.MaxExpansionTermCount.As(num, () =>
                {
                    for (int i = 0; i < 1000; i++)
                        Assert.Equal(num, MathS.Settings.MaxExpansionTermCount.Value);
                });
            new ThreadingChecker(Checker).Run(iterCount: 100);
        }

        [Fact]
        public void WithExceptionInside()
        {
            MathS.Settings.MaxExpansionTermCount.As(500, () =>
                {
                    Assert.Equal(500, MathS.Settings.MaxExpansionTermCount.Value);
                    Assert.Throws<DataMisalignedException>(() =>
                        MathS.Settings.MaxExpansionTermCount.As(300, () =>
                            // something happens
                            throw new DataMisalignedException() // some random exception
                        ));
                    // should be kept as 500
                    Assert.Equal(500, MathS.Settings.MaxExpansionTermCount.Value);
                }
            );
        }
    }
}
