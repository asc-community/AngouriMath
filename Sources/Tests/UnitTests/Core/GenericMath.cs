using AngouriMath;
using AngouriMath.Extensions;
using System;
using Xunit;

namespace UnitTests.Core
{
    public class GenericMath
    {
        [Fact]
        public void TestingParse()
        {
            Assert.Equal(55, Quack<int>("55"));
            Assert.Equal("x + a".ToEntity(), Quack<Entity>("x + a"));

            static T Quack<T>(string a) where T : IParseable<T>
                => T.Parse(a, null);
        }

        [Fact]
        public void TestingEquality()
        {
            Assert.True(Quack(55, 55));
            Assert.True(Quack("x + a".ToEntity(), "x + a".ToEntity()));

            static bool Quack<T>(T a, T b) where T : IEqualityOperators<T, T>
                => a == b;
        }

        [Fact]
        public void TestingUnaryNegation()
        {
            Assert.Equal(55, Quack(-55));
            Assert.Equal("-x".ToEntity(), Quack("x".ToEntity()));

            static T Quack<T>(T a) where T : IUnaryNegationOperators<T, T>
                => -a;
        }

        [Fact]
        public void TestingPlus()
        {
            Assert.Equal(55, Quack(55));
            Assert.Equal("x".ToEntity(), Quack("x".ToEntity()));

            static T Quack<T>(T a) where T : IUnaryPlusOperators<T, T>
                => +a;
        }

        [Fact]
        public void TestingAddition()
        {
            Assert.Equal(60, Quack(55, 5));
            Assert.Equal("a + b", Quack<Entity>("a", "b"));

            static T Quack<T>(T a, T b) where T : IAdditionOperators<T, T, T>
                => a + b;
        }

        [Fact]
        public void TestingSubtraction()
        {
            Assert.Equal(50, Quack(55, 5));
            Assert.Equal("a - b", Quack<Entity>("a", "b"));

            static T Quack<T>(T a, T b) where T : ISubtractionOperators<T, T, T>
                => a - b;
        }

        [Fact]
        public void TestingMultiply()
        {
            Assert.Equal(160, Quack(32, 5));
            Assert.Equal("a * b", Quack<Entity>("a", "b"));

            static T Quack<T>(T a, T b) where T : IMultiplyOperators<T, T, T>
                => a * b;
        }

        [Fact]
        public void TestingDivison()
        {
            Assert.Equal(11, Quack(55, 5));
            Assert.Equal("a / b", Quack<Entity>("a", "b"));

            static T Quack<T>(T a, T b) where T : IDivisionOperators<T, T, T>
                => a / b;
        }

        [Fact]
        public void TestingAdditiveIdentity()
        {
            Assert.Equal(55, Quack(55));
            Assert.Equal("0 + a", Quack<Entity>("a"));

            static T Quack<T>(T a) where T : IAdditiveIdentity<T, T>, IAdditionOperators<T, T, T>
                => T.AdditiveIdentity + a;
        }

        [Fact]
        public void TestingMultiplicativeIdentity()
        {
            Assert.Equal(55, Quack(55));
            Assert.Equal("1 * a", Quack<Entity>("a"));

            static T Quack<T>(T a) where T : IMultiplicativeIdentity<T, T>, IMultiplyOperators<T, T, T>
                => T.MultiplicativeIdentity * a;
        }
    }
}
