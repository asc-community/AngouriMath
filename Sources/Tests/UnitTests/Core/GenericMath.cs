using AngouriMath;
using System;
using Xunit;

namespace UnitTests.Core
{
    public class GenericMath
    {
        [Fact]
        public void TestingParse()
        {
            Assert.Equal(QuackParse<int>("55"), 55);
            Assert.Equal(QuackParse<Entity>("x + a"), (Entity)"x + a");

            static T QuackParse<T>(string a) where T : IParseable<T>
                => T.Parse(a, null);
        }
    }
}
