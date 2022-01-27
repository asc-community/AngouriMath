//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using HonkSharp.Fluency;
using Xunit;
using FluentAssertions;
using PeterO.Numbers;
using static AngouriMath.Entity.Number;

namespace AngouriMath.Tests.Core
{
    public sealed class NumericDowncastingTest
    {
        #region Test from complex to...
        [Fact] public void FromComplexTestComplexDowncastingDisabledInt()
            => MathS.Settings.DowncastingEnabled.As(false, 
                () => 24
                    .Pipe(a => (Complex)a)
                    .Should().BeOfType<Complex>()
            );
        
        [Fact] public void FromComplexTestComplexDowncastingEnabledInt()
            => MathS.Settings.DowncastingEnabled.As(true, 
                () => 24
                    .Pipe(a => (Complex)a)
                    .Should().BeOfType<Integer>()
            );
        
        [Fact] public void FromComplexTestComplexDowncastingEnabledInteger()
            => MathS.Settings.DowncastingEnabled.As(true, 
                () => ((EInteger)23)
                    .Pipe(a => (Complex)a)
                    .Should().BeOfType<Integer>()
            );
        
        [Fact] public void FromComplexTestComplexDowncastingDisabledInteger()
            => MathS.Settings.DowncastingEnabled.As(false, 
                () => ((EInteger)23)
                    .Pipe(a => (Complex)a)
                    .Should().BeOfType<Complex>()
            );
        
        [Fact] public void FromComplexTestComplexDowncastingEnabledRational()
            => MathS.Settings.DowncastingEnabled.As(true, 
                () => ERational.Create(5, 4)
                    .Pipe(a => (Complex)a)
                    .Should().BeOfType<Rational>()
            );
        
        [Fact] public void FromComplexTestComplexDowncastingDisabledRational()
            => MathS.Settings.DowncastingEnabled.As(false, 
                () => ERational.Create(5, 4)
                    .Pipe(a => (Complex)a)
                    .Should().BeOfType<Complex>()
            );
        
        [Fact] public void FromComplexTestComplexDowncastingEnabledReal()
            => MathS.Settings.DowncastingEnabled.As(true, 
                () => EDecimal.FromString("4.44")
                    .Pipe(a => (Complex)a)
                    .Should().BeOfType<Rational>()
            );
        
        [Fact] public void FromComplexTestComplexDowncastingDisabledReal()
            => MathS.Settings.DowncastingEnabled.As(false, 
                () => EDecimal.FromString("4.44")
                    .Pipe(a => (Complex)a)
                    .Should().BeOfType<Complex>()
            );
        
        [Fact] public void FromComplexTestComplexDowncastingDisabledSum()
            => MathS.Settings.DowncastingEnabled.As(false, 
                () => ((Complex)24, (Complex)45)
                    .Pipe((a, b) => a + b)
                    .Should().BeOfType<Complex>()
            );
        
        [Fact] public void FromComplexTestComplexDowncastingEnabledSum()
            => MathS.Settings.DowncastingEnabled.As(true, 
                () => ((Complex)24, (Complex)45)
                    .Pipe((a, b) => a + b)
                    .Should().BeOfType<Integer>()
            );
        
        [Fact] public void FromComplexTestComplexDowncastingDisabledSubtraction()
            => MathS.Settings.DowncastingEnabled.As(false, 
                () => ((Complex)24, (Complex)45)
                    .Pipe((a, b) => a - b)
                    .Should().BeOfType<Complex>()
            );
        
        [Fact] public void FromComplexTestComplexDowncastingEnabledSubtraction()
            => MathS.Settings.DowncastingEnabled.As(true, 
                () => ((Complex)24, (Complex)45)
                    .Pipe((a, b) => a - b)
                    .Should().BeOfType<Integer>()
            );
        
        [Fact] public void FromComplexTestComplexDowncastingDisabledProduct()
            => MathS.Settings.DowncastingEnabled.As(false, 
                () => ((Complex)24, (Complex)45)
                    .Pipe((a, b) => a * b)
                    .Should().BeOfType<Complex>()
            );
        
        [Fact] public void FromComplexTestComplexDowncastingEnabledProduct()
            => MathS.Settings.DowncastingEnabled.As(true, 
                () => ((Complex)24, (Complex)45)
                    .Pipe((a, b) => a * b)
                    .Should().BeOfType<Integer>()
            );
        
        [Fact]
        public void FromComplexTestComplexDowncastingDisabledDivision()
            => MathS.Settings.DowncastingEnabled.As(false, 
                () => ((Complex)24, (Complex)45)
                    .Pipe((a, b) => a / b)
                    .Should().BeOfType<Complex>()
            );
        
        [Fact] public void FromComplexTestComplexDowncastingEnabledDivision()
            => MathS.Settings.DowncastingEnabled.As(true, 
                () => ((Complex)24, (Complex)45)
                    .Pipe((a, b) => a / b)
                    .Should().BeOfType<Rational>()
            );
        
        [Fact] public void FromComplexTestComplexDowncastingDisabledPower()
            => MathS.Settings.DowncastingEnabled.As(false, 
                () => MathS.Sqrt(4)
                    .Evaled
                    .Should().BeOfType<Complex>()
            );
        
        [Fact] public void FromComplexTestComplexDowncastingEnabledPower()
            => MathS.Settings.DowncastingEnabled.As(true, 
                () => MathS.Sqrt(4)
                    .Evaled
                    .Should().BeOfType<Integer>()
            );
        #endregion
        
        #region Test from real to...
        [Fact] public void FromRealTestRealDowncastingDisabledInt()
            => MathS.Settings.DowncastingEnabled.As(false, 
                () => 24
                    .Pipe(a => (Real)a)
                    .Should().BeOfType<Real>()
            );
        
        [Fact] public void FromRealTestRealDowncastingEnabledInt()
            => MathS.Settings.DowncastingEnabled.As(true, 
                () => 24
                    .Pipe(a => (Real)a)
                    .Should().BeOfType<Integer>()
            );
        
        [Fact] public void FromRealTestRealDowncastingEnabledInteger()
            => MathS.Settings.DowncastingEnabled.As(true, 
                () => ((EInteger)23)
                    .Pipe(a => (Real)a)
                    .Should().BeOfType<Integer>()
            );
        
        [Fact] public void FromRealTestRealDowncastingDisabledInteger()
            => MathS.Settings.DowncastingEnabled.As(false, 
                () => ((EInteger)23)
                    .Pipe(a => (Real)a)
                    .Should().BeOfType<Real>()
            );
        
        [Fact] public void FromRealTestRealDowncastingEnabledRational()
            => MathS.Settings.DowncastingEnabled.As(true, 
                () => ERational.Create(5, 4)
                    .Pipe(a => (Real)a)
                    .Should().BeOfType<Rational>()
            );
        
        [Fact] public void FromRealTestRealDowncastingDisabledRational()
            => MathS.Settings.DowncastingEnabled.As(false, 
                () => ERational.Create(5, 4)
                    .Pipe(a => (Real)a)
                    .Should().BeOfType<Real>()
            );
        
        [Fact] public void FromRealTestRealDowncastingDisabledSum()
            => MathS.Settings.DowncastingEnabled.As(false, 
                () => ((Real)24, (Real)45)
                    .Pipe((a, b) => a + b)
                    .Should().BeOfType<Real>()
            );
        
        [Fact] public void FromRealTestRealDowncastingEnabledSum()
            => MathS.Settings.DowncastingEnabled.As(true, 
                () => ((Real)24, (Real)45)
                    .Pipe((a, b) => a + b)
                    .Should().BeOfType<Integer>()
            );
        
        [Fact] public void FromRealTestRealDowncastingDisabledSubtraction()
            => MathS.Settings.DowncastingEnabled.As(false, 
                () => ((Real)24, (Real)45)
                    .Pipe((a, b) => a - b)
                    .Should().BeOfType<Real>()
            );
        
        [Fact] public void FromRealTestRealDowncastingEnabledSubtraction()
            => MathS.Settings.DowncastingEnabled.As(true, 
                () => ((Real)24, (Real)45)
                    .Pipe((a, b) => a - b)
                    .Should().BeOfType<Integer>()
            );
        
        [Fact] public void FromRealTestRealDowncastingDisabledProduct()
            => MathS.Settings.DowncastingEnabled.As(false, 
                () => ((Real)24, (Real)45)
                    .Pipe((a, b) => a * b)
                    .Should().BeOfType<Real>()
            );
        
        [Fact] public void FromRealTestRealDowncastingEnabledProduct()
            => MathS.Settings.DowncastingEnabled.As(true, 
                () => ((Real)24, (Real)45)
                    .Pipe((a, b) => a * b)
                    .Should().BeOfType<Integer>()
            );
        
        [Fact] public void FromRealTestRealDowncastingDisabledDivision()
            => MathS.Settings.DowncastingEnabled.As(false, 
                () => ((Real)24, (Real)45)
                    .Pipe((a, b) => a / b)
                    .Should().BeOfType<Real>()
            );
        
        [Fact] public void FromRealTestRealDowncastingEnabledDivision()
            => MathS.Settings.DowncastingEnabled.As(true, 
                () => ((Real)24, (Real)45)
                    .Pipe((a, b) => a / b)
                    .Should().BeOfType<Rational>()
            );
        
        [Fact] public void FromRealTestRealDowncastingEnabledPower()
            => MathS.Settings.DowncastingEnabled.As(true, 
                () => MathS.Sqrt(4)
                    .Evaled
                    .Should().BeOfType<Integer>()
            );
        #endregion

        #region Test from rational to..
        
        [Fact] public void FromRationalTestRealDowncastingDisabledInt()
            => MathS.Settings.DowncastingEnabled.As(false, 
                () => 24
                    .Pipe(a => (Rational)a)
                    .Should().BeOfType<Rational>()
            );
        
        [Fact] public void FromRationalTestRealDowncastingEnabledInt()
            => MathS.Settings.DowncastingEnabled.As(true, 
                () => 24
                    .Pipe(a => (Rational)a)
                    .Should().BeOfType<Integer>()
            );
        
        #endregion
    }
}