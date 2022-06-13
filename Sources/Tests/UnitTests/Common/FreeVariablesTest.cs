//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Collections.Generic;
using System.Linq;
using AngouriMath;
using AngouriMath.Extensions;
using HonkSharp.Fluency;
using Xunit;

namespace AngouriMath.Tests.Common
{
    public class FreeVariablesTest
    {
        private IEnumerable<Entity.Variable> SeqVar(params string[] vars)
            => vars.Select(MathS.Var);

        [Fact]
        public void Test1()
            => Assert.Equal(
                SeqVar("x", "y"),
                "x + y".ToEntity().FreeVariables
            );
        
        [Fact]
        public void Test2()
            => Assert.Equal(
                SeqVar("y"),
                "lambda(x, x + y)".ToEntity().FreeVariables
            );
        
        [Fact]
        public void Test3()
            => Assert.Equal(
                SeqVar(),
                "lambda(x, lambda(y, x + y))".ToEntity().FreeVariables
            );
            
        [Fact]
        public void Test4()
            => Assert.Equal(
                SeqVar(),
                "lambda(x, lambda(y, x + y))".ToEntity().FreeVariables
            );
            
        [Fact]
        public void Test5()
            => Assert.Equal(
                SeqVar(),
                "lambda(x, lambda(y, x + y))".ToEntity().FreeVariables
            );
            
        [Fact]
        public void Test6()
            => Assert.Equal(
                SeqVar("x", "y", "z"),
                "x + lambda(x, y + z + x)".ToEntity().FreeVariables
            );
        
        [Fact]
        public void Test7()
            => Assert.Equal(
                SeqVar("x", "y", "z"),
                "x + lambda(f, y + z + x)".ToEntity().FreeVariables
            );
    }
}