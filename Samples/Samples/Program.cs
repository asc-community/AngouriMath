

using AngouriMath;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using static AngouriMath.Entity;

namespace Samples
{
    public record A
    {
        public A? Another => another.GetValue(this, e => e);
        private static readonly ConditionalWeakTable<A, A> another = new();

        record Wrapper<T>(T Value) { }
    }

   
    class Program
    {
        static void Main(string[] _)
        {
            Variable x = "x";
            var expr = x;
            var y = (expr.DefiniteIntegral(x, 0, 1).RealPart - 1.0 / 2).Abs() < 0.1;

        }
    }
}