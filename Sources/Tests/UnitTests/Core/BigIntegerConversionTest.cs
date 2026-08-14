//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Numerics;
using AngouriMath;
using Xunit;

namespace AngouriMath.Tests.Core
{
    /// <summary>
    /// The two conversions from <see cref="BigInteger"/>, which used to disagree: the one to
    /// <see cref="Entity"/> read the bytes and the one to <see cref="Entity.Number"/> parsed
    /// them as text, so the second threw on nearly every input.
    /// </summary>
    [Trait("Area", "Core")]
    public sealed class BigIntegerConversionTest
    {
        public static readonly TheoryData<string> Values = new()
        {
            "0", "1", "-1", "2", "-2", "127", "128", "255", "256", "-128", "-129",
            "123456789", "-123456789", "9223372036854775807", "9223372036854775808",
            "-9223372036854775808", "-9223372036854775809",
            "170141183460469231731687303715884105728",
            "-170141183460469231731687303715884105728",
        };

        /// <summary>To <see cref="Entity.Number"/>, which is the one that was broken.</summary>
        [Theory]
        [MemberData(nameof(Values))]
        public void ToNumber(string text)
        {
            Entity.Number converted = BigInteger.Parse(text);
            Assert.Equal(text, converted.Stringize());
        }

        /// <summary>And to <see cref="Entity"/>, which was not, pinned so it stays that way.</summary>
        [Theory]
        [MemberData(nameof(Values))]
        public void ToEntity(string text)
        {
            Entity converted = BigInteger.Parse(text);
            Assert.Equal(text, converted.Stringize());
        }

        /// <summary>
        /// And the two agree, which is the property whose absence was the defect: one conversion
        /// of one value should not depend on which of them the caller reaches.
        /// </summary>
        [Theory]
        [MemberData(nameof(Values))]
        public void AndTheTwoAgree(string text)
        {
            var big = BigInteger.Parse(text);
            Entity asEntity = big;
            Entity.Number asNumber = big;
            Assert.Equal(asEntity, (Entity)asNumber);
        }

        /// <summary>
        /// A value whose bytes happen to be ASCII digits was the only kind that used to work,
        /// and it worked by accident: 12594 is the two bytes '2' and '1', which the old code
        /// read as the number 21.
        /// </summary>
        [Fact]
        public void TheValuesThatUsedToWorkWorkedByAccident()
        {
            Entity.Number converted = new BigInteger(12594);
            Assert.Equal("12594", converted.Stringize());
        }
    }
}
