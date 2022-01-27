//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using Xunit;
using AngouriMath.Extensions;
using System.Threading.Tasks;
using System.Threading;

namespace AngouriMath.Tests.Core.Multithreading
{
    public sealed class MultithreadedTest
    {
        [Fact]
        public async void Single1()
        {
            var token = new CancellationTokenSource().Token;
            MathS.Multithreading.SetLocalCancellationToken(token);
            var res = await Task.Run(() => "3 + 2".EvalNumerical(), token);
            Assert.Equal(MathS.FromString("5"), res);
        }

        [Fact]
        public async void Single2()
        {
            var token = new CancellationTokenSource().Token;
            MathS.Multithreading.SetLocalCancellationToken(token);
            var res = await Task.Run(() => "1 + 2 + 3 + 4 + 5".EvalNumerical(), token);
            Assert.Equal(MathS.FromString("15"), res);
        }

        [Fact]
        public async void Multiple2()
        {
            var token1 = new CancellationTokenSource().Token;
            var task1 = Task.Run(
                () =>
                {
                    MathS.Multithreading.SetLocalCancellationToken(token1);
                    return "a sin(x2 + x)2 + b sin(x2 + x) + c = 0".Solve("x");
                });
            var token2 = new CancellationTokenSource().Token;
            var task2 = Task.Run(
                () =>
                {
                    MathS.Multithreading.SetLocalCancellationToken(token2);
                    return "f sin(x2 + x)2 + d sin(x2 + x) + g = 0".Solve("x");
                    }, new CancellationTokenSource().Token);
            var results = await Task.WhenAll(task1, task2);
            Assert.NotEqual(MathS.Sets.Empty, results[0]);
            Assert.NotEqual(MathS.Sets.Empty, results[1]);  
        }
    }
}
