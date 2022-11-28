//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using System;
using System.Numerics;
using Xunit;
using static AngouriMath.Entity.Number;
using Complex = AngouriMath.Entity.Number.Complex;

namespace AngouriMath.Tests;

public class GenericMath
{
    [Fact]
    public void TestingParse()
    {
        Assert.Equal(55, Quack<int>("55"));
        Assert.Equal("x + a".ToEntity(), Quack<Entity>("x + a"));

        static T Quack<T>(string a) where T : IParsable<T>
            => T.Parse(a, null);
    }

    [Fact]
    public void TestingEquality()
    {
        Assert.True(Quack(55, 55));
        Assert.True(Quack("x + a".ToEntity(), "x + a".ToEntity()));

        static bool Quack<T>(T a, T b) where T : IEqualityOperators<T, T, bool>
            => a == b;
    }

    [Fact]
    public void TestingUnaryNegation()
    {
        Assert.Equal(55, Quack(-55));
        Assert.Equal("-x".ToEntity(), Quack("x".ToEntity()));
        Assert.Equal(-10, Quack<Integer>(10));
        Assert.Equal(Rational.Create(-2, 5), Quack(Rational.Create(2, 5)));
        Assert.Equal(-0.1318392429, Quack<Real>(0.1318392429));
        Assert.Equal(Complex.Create(-4, -6), Quack<Complex>(Complex.Create(4, 6)));

        static T Quack<T>(T a) where T : IUnaryNegationOperators<T, T>
            => -a;
    }

    [Fact]
    public void TestingPlus()
    {
        Assert.Equal(55, Quack(55));
        Assert.Equal("x".ToEntity(), Quack("x".ToEntity()));
        Assert.Equal(10, Quack<Integer>(10));
        Assert.Equal(Rational.Create(2, 5), Quack(Rational.Create(2, 5)));
        Assert.Equal(0.1318392429, Quack<Real>(0.1318392429));
        Assert.Equal(Complex.Create(4, 6), Quack<Complex>(Complex.Create(4, 6)));

        static T Quack<T>(T a) where T : IUnaryPlusOperators<T, T>
            => +a;
    }

    [Fact]
    public void TestingAddition()
    {
        Assert.Equal(60, Quack(55, 5));
        Assert.Equal("a + b", Quack<Entity>("a", "b"));
        Assert.Equal(15, Quack<Integer>(10, 5));
        Assert.Equal(Rational.Create(3, 5), Quack(Rational.Create(2, 5), Rational.Create(1, 5)));
        Assert.Equal(0.3m, Quack<Real>(0.1m, 0.2m));
        Assert.Equal(Complex.Create(8, 7), Quack<Complex>(Complex.Create(4, 6), Complex.Create(4, 1)));
        Assert.Equal(MathS.Matrix(new Entity[,] { { 1, 2 }, { 3, 4 } }), 
            Quack(MathS.Matrix(new Entity[,] { { 0, 1 }, { 2, 3 } }), MathS.Matrix(new Entity[,] { { 1, 1 }, { 1, 1 } })));

        static T Quack<T>(T a, T b) where T : IAdditionOperators<T, T, T>
            => a + b;
    }

    [Fact]
    public void TestingSubtraction()
    {
        Assert.Equal(50, Quack(55, 5));
        Assert.Equal("a - b", Quack<Entity>("a", "b"));
        Assert.Equal(5, Quack<Integer>(10, 5));
        Assert.Equal(Rational.Create(1, 5), Quack(Rational.Create(2, 5), Rational.Create(1, 5)));
        Assert.Equal(-0.1, Quack<Real>(0.1, 0.2));
        Assert.Equal(Complex.Create(0, 5), Quack<Complex>(Complex.Create(4, 6), Complex.Create(4, 1)));
        Assert.Equal(MathS.Matrix(new Entity[,] { { -1, 0 }, { 1, 2 } }),
            Quack(MathS.Matrix(new Entity[,] { { 0, 1 }, { 2, 3 } }), MathS.Matrix(new Entity[,] { { 1, 1 }, { 1, 1 } })));

        static T Quack<T>(T a, T b) where T : ISubtractionOperators<T, T, T>
            => a - b;
    }

    [Fact]
    public void TestingMultiply()
    {
        Assert.Equal(160, Quack(32, 5));
        Assert.Equal("a * b", Quack<Entity>("a", "b"));
        Assert.Equal(50, Quack<Integer>(10, 5));
        Assert.Equal(Rational.Create(2, 25), Quack(Rational.Create(2, 5), Rational.Create(1, 5)));
        Assert.Equal(0.02m, Quack<Real>(0.1m, 0.2m));
        Assert.Equal(Complex.Create(16, 24), Quack<Complex>(Complex.Create(4, 6), Complex.Create(4, 0)));
        Assert.Equal(MathS.Matrix(new Entity[,] { { 1, 1 }, { 5, 5 } }),
            Quack(MathS.Matrix(new Entity[,] { { 0, 1 }, { 2, 3 } }), MathS.Matrix(new Entity[,] { { 1, 1 }, { 1, 1 } })));

        static T Quack<T>(T a, T b) where T : IMultiplyOperators<T, T, T>
            => a * b;
    }

    [Fact]
    public void TestingDivison()
    {
        Assert.Equal(11, Quack(55, 5));
        Assert.Equal("a / b", Quack<Entity>("a", "b"));
        Assert.Equal(2, OhNo<Integer, Real>(10, 5));
        Assert.Equal(2, OhNo<Rational, Real>(Rational.Create(2, 5), Rational.Create(1, 5)));
        Assert.Equal(0.5, Quack<Real>(0.1, 0.2));
        Assert.Equal(Complex.Create(1, 1.5), Quack<Complex>(Complex.Create(4, 6), Complex.Create(4, 0)));

        static T Quack<T>(T a, T b) where T : IDivisionOperators<T, T, T>
            => a / b;
        static TOut OhNo<T, TOut>(T a, T b) where T : IDivisionOperators<T, T, TOut>
            => a / b;
    }

    [Fact]
    public void TestingAdditiveIdentity()
    {
        Assert.Equal(55, Quack(55));
        Assert.Equal("0 + a", Quack<Entity>("a"));
        Assert.Equal(10, Quack<Integer>(10));
        Assert.Equal(Rational.Create(2, 5), Quack(Rational.Create(2, 5)));
        Assert.Equal(0.1318392429, Quack<Real>(0.1318392429));
        Assert.Equal(Complex.Create(4, 6), Quack<Complex>(Complex.Create(4, 6)));

        static T Quack<T>(T a) where T : IAdditiveIdentity<T, T>, IAdditionOperators<T, T, T>
            => T.AdditiveIdentity + a;
    }

    [Fact]
    public void TestingMultiplicativeIdentity()
    {
        Assert.Equal(55, Quack(55));
        Assert.Equal("1 * a", Quack<Entity>("a"));
        Assert.Equal(10, Quack<Integer>(10));
        Assert.Equal(Rational.Create(2, 5), Quack(Rational.Create(2, 5)));
        Assert.Equal(0.1318392429, Quack<Real>(0.1318392429));
        Assert.Equal(Complex.Create(4, 6), Quack<Complex>(Complex.Create(4, 6)));

        static T Quack<T>(T a) where T : IMultiplicativeIdentity<T, T>, IMultiplyOperators<T, T, T>
            => T.MultiplicativeIdentity * a;
    }
}
