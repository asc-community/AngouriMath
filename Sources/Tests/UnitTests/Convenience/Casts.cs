//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using Xunit;
using AngouriMath.Extensions;
using System.Numerics;
using AngouriMath.Core.Exceptions;

namespace AngouriMath.Tests.Convenience
{
    public sealed class Casts
    {
        [Fact] public void ToDoubleSuccess1() => Assert.Equal(3d, (double)"3".EvalNumerical());
        [Fact] public void ToDoubleSuccess2() => Assert.Equal(3.4d, (double)"3 + 2 / 5".EvalNumerical());
        [Fact] public void ToFloatSuccess1() => Assert.Equal(3f, (float)"3".EvalNumerical());
        [Fact] public void ToFloatSuccess2() => Assert.Equal(3.4f, (float)"3 + 2 / 5".EvalNumerical());
        [Fact] public void ToIntSuccess1() => Assert.Equal(3, (int)"3".EvalNumerical());
        [Fact] public void ToIntSuccess2() => Assert.Equal(-10, (int)"3 - 4 - 9".EvalNumerical());
        [Fact] public void ToInt64Success3() => Assert.Equal(3, (long)"16 / 5".EvalNumerical());
        [Fact] public void ToInt64Success1() => Assert.Equal(3, (long)"3".EvalNumerical());
        [Fact] public void ToInt64Success2() => Assert.Equal(-10, (long)"3 - 4 - 9".EvalNumerical());
        [Fact] public void ToBigIntegerSuccess1() => Assert.Equal(BigInteger.Parse("32758732583275823578"), (BigInteger)"32758732583275823578".EvalNumerical());
        [Fact] public void ToComplexSuccess1() => Assert.Equal(new Complex(1, 2), (Complex)"1 + 2i".EvalNumerical());
        [Fact] public void ToComplexSuccess2() => Assert.Equal(new Complex(1, 0.5), (Complex)"1 + 1/2 * i".EvalNumerical());

        [Fact] public void ToDoubleUnsuc1() => Assert.Throws<NumberCastException>(() => (double)"1 + i".EvalNumerical());
        [Fact] public void ToDoubleUnsuc2() => Assert.Throws<NumberCastException>(() => (double)"1 + i / 2".EvalNumerical());
    }
}
