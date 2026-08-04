//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using AngouriMath;
using AngouriMath.Core.Exceptions;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Common
{
    /// <summary>
    /// Compiling an expression over some of its variables and not the rest. No issue is
    /// filed for this; it was found sweeping the compilers for expressions that fail in a
    /// way that says nothing.
    /// </summary>
    public sealed class CompileUnboundVariableTest
    {
        /// <summary>
        /// A compiled expression is a function of the variables it was compiled over, so one
        /// it does not mention cannot be given a value. Both compilers reached into a
        /// dictionary and let the lookup fail, which raised a KeyNotFoundException saying
        /// only that some key was not present in some dictionary, naming neither the
        /// variable nor the fact that a compilation was going on.
        /// </summary>
        [Theory]
        [InlineData("a * x", "a")]
        [InlineData("x + y", "y")]
        [InlineData("sin(x) + b", "b")]
        [InlineData("x ^ 2 + c * x + 1", "c")]
        public void TheFastCompilerNamesTheVariableItWasNotGiven(string expression, string missing)
        {
            var thrown = Assert.Throws<UncompilableNodeException>(
                () => expression.ToEntity().Compile("x"));
            Assert.Contains(missing, thrown.Message);
            Assert.Contains("x", thrown.Message);
        }

        [Theory]
        [InlineData("a * x", "a")]
        [InlineData("x + y", "y")]
        [InlineData("sin(x) + b", "b")]
        public void TheLinqCompilerNamesTheVariableItWasNotGiven(string expression, string missing)
        {
            var thrown = Assert.Throws<UncompilableNodeException>(
                () => expression.ToEntity().Compile<double, double>("x"));
            Assert.Contains(missing, thrown.Message);
        }

        [Fact]
        public void CompilingOverNothingSaysSo()
        {
            var thrown = Assert.Throws<UncompilableNodeException>(
                () => "y".ToEntity().Compile(Array.Empty<Entity.Variable>()));
            Assert.Contains("y", thrown.Message);
            Assert.Contains("none", thrown.Message);
        }

        // Everything that compiled before still does, constants included: pi and e are
        // substituted for their values before any variable is looked up, so they are not
        // variables the caller has to pass.
        [Theory]
        [InlineData("x + 2", 3.0)]
        [InlineData("pi * x", Math.PI)]
        [InlineData("e ^ x", Math.E)]
        [InlineData("sin(x) ^ 2 + cos(x) ^ 2", 1.0)]
        public void WhatCompiledBeforeStillCompiles(string expression, double atOne)
        {
            var compiled = expression.ToEntity().Compile("x");
            Assert.Equal(atOne, compiled.Call(1.0).Real, 8);
        }

        [Fact]
        public void EveryVariableGivenIsEnough()
        {
            var compiled = "x + y".ToEntity().Compile("x", "y");
            Assert.Equal(3.0, compiled.Call(1.0, 2.0).Real, 8);
            var linq = "x + y".ToEntity().Compile<double, double, double>("x", "y");
            Assert.Equal(3.0, linq(1.0, 2.0), 8);
        }
    }
}
