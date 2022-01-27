//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using AngouriMath;
using AngouriMath.Extensions;
using Xunit;
using static AngouriMath.Entity;

namespace AngouriMath.Tests.Core.Multithreading
{
    public sealed class SettingsAndThreads
    {
        #region Obsolete settings
#pragma warning disable CS0618 // The obsolete method should still work!
        [Fact]
        public void OSettingValue1()
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
        public void OSettingValue2()
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

        [Fact]
        public void OSettingParallelSolve()
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
        public void OSettingParallelSeparateUse()
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
        public void OWithExceptionInside()
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
#pragma warning restore CS0618 // The obsolote method should still work!
        #endregion

        #region Normal settings
        [Fact]
        public void SettingValue1()
        {
            using var _ = MathS.Settings.MaxExpansionTermCount.Set(27);
            Assert.Equal(27, MathS.Settings.MaxExpansionTermCount.Value);

            using (var __ = MathS.Settings.MaxExpansionTermCount.Set(134))
                Assert.Equal(134, MathS.Settings.MaxExpansionTermCount.Value);

            Assert.Equal(27, MathS.Settings.MaxExpansionTermCount.Value);
        }

        [Fact]
        public void SettingValue2()
        {
            using var _ = MathS.Settings.PrecisionErrorCommon.Set(27.4m);
            Assert.Equal(27.4m, MathS.Settings.PrecisionErrorCommon.Value);

            using (var __ = MathS.Settings.PrecisionErrorCommon.Set(134.5m))
                Assert.Equal(134.5m, MathS.Settings.PrecisionErrorCommon.Value);

            Assert.Equal(27.4m, MathS.Settings.PrecisionErrorCommon.Value);
        }

        [Fact]
        public void SettingParallelSolve()
        {
            static void Solve(int num)
            {
                using var _ = MathS.Settings.AllowNewton.Set(num % 2 == 0);
                var roots = "x2 + e^x + sin(x)".SolveEquation("x");
                var set = (Set)roots.Simplify();
                Assert.Equal(num % 2 == 0, !set.IsSetEmpty);
            }
            new ThreadingChecker(Solve).Run();
        }

        [Fact]
        public void SettingParallelSeparateUse()
        {
            static void Checker(int num)
            {
                using var _ = MathS.Settings.MaxExpansionTermCount.Set(num);
                for (int i = 0; i < 1000; i++)
                    Assert.Equal(num, MathS.Settings.MaxExpansionTermCount.Value);
            }
            new ThreadingChecker(Checker).Run(iterCount: 100);
        }

        [Fact]
        public void WithExceptionInside()
        {

            using var _ = MathS.Settings.MaxExpansionTermCount.Set(500);
            Assert.Equal(500, MathS.Settings.MaxExpansionTermCount.Value);
            Assert.Throws<DataMisalignedException>((Action)(() =>
                {
                    using var _ = MathS.Settings.MaxExpansionTermCount.Set(300);
                    throw new DataMisalignedException();
                })
                );
            // should be kept as 500
            Assert.Equal(500, MathS.Settings.MaxExpansionTermCount.Value);
        }
        #endregion



    }
}
