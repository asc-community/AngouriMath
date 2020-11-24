using AngouriMath;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using AngouriMath.Extensions;
using System.Threading.Tasks;
using System.Threading;

namespace UnitTests.Core
{
    public sealed class MultithreadedTest
    {
        [Fact]
        public async void Single1()
        {
            var res = await MathS.Multithreading.RunAsync(() => "3 + 2".EvalNumerical(), new CancellationTokenSource().Token);
            Assert.Equal(MathS.FromString("5"), res);
        }

        [Fact]
        public async void Single2()
        {
            var res = await MathS.Multithreading.RunAsync(() => "1 + 2 + 3 + 4 + 5".EvalNumerical(), new CancellationTokenSource().Token);
            Assert.Equal(MathS.FromString("15"), res);
        }

        [Fact]
        public async void Multiple2()
        {
            var task1 = MathS.Multithreading.RunAsync(() => "a sin(x2 + x)2 + b sin(x2 + x) + c = 0".Solve("x"), new CancellationTokenSource().Token);
            var task2 = MathS.Multithreading.RunAsync(() => "f sin(x2 + x)2 + d sin(x2 + x) + g = 0".Solve("x"), new CancellationTokenSource().Token);
            var results = await Task.WhenAll(task1, task2);
            Assert.NotEqual(MathS.Sets.Empty, results[0]);
            Assert.NotEqual(MathS.Sets.Empty, results[1]);
        }
    }
}
