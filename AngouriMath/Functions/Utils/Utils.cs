
/* Copyright (c) 2019-2020 Angourisoft
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,
 * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software
 * is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AngouriMath.Core.Exceptions;
using AngouriMath.Core.TreeAnalysis;
using AngouriMath.Functions;
using AngouriMath.Functions.Algebra.AnalyticalSolving;
using PeterO.Numbers;

namespace AngouriMath.Functions
{
    using static Entity;
    using static Entity.Number;
    internal static class Utils
    {
        // TODO: Using double for operations loses precision from 100 digits to 15 digits. This won't work.
        private static readonly EDecimal LowerForGood = EDecimal.FromDecimal(1e-6m);
        private static readonly EDecimal UpperForGood = EDecimal.FromDecimal(1e+8m);
        internal static bool IsGoodAsDouble(EDecimal num)
            => num.CompareTo(LowerForGood) > 0 && num.CompareTo(UpperForGood) < 0;

        /// <summary>
        /// Sorts an expression into a polynomial.
        /// See more at <see cref="MathS.Utils.TryPolynomial"/>
        /// </summary>
        internal static bool TryPolynomial(Entity expr, Variable variable,
            [System.Diagnostics.CodeAnalysis.NotNullWhen(true)]
            out Entity? dst)
        {
            dst = null;
            var children = Sumf.LinearChildren(expr.Expand());
            var monomialsByPower = PolynomialSolver.GatherMonomialInformation
                <EInteger, TreeAnalyzer.PrimitiveInteger>(children, variable);
            if (monomialsByPower == null)
                return false;
            var newMonomialsByPower = new Dictionary<int, Entity>();
            var terms = new List<Entity>();
            foreach (var index in monomialsByPower.Keys.OrderByDescending(x => x))
            {
                var pair = new KeyValuePair<EInteger, Entity>(index, monomialsByPower[index]);
                if (pair.Key.IsZero)
                {
                    terms.Add(pair.Value.Simplify());
                    continue;
                }

                var px = pair.Key.Equals(EInteger.One) ? variable : MathS.Pow(variable, pair.Key);
                if (pair.Value == 1)
                {
                    terms.Add(px);
                    continue;
                }
                else
                    terms.Add(pair.Value.Simplify() * px);
            }

            if (terms.Count == 0)
                return false;
            dst = terms[0];
            for (int i = 1; i < terms.Count; i++)
                if (terms[i] is Mulf(Real { IsNegative:true } r, var m))
                    dst -= -r * m;
                else
                    dst += terms[i];
            dst = dst.InnerSimplify();
            return true;
        }
    }

    public class Setting<T> where T : notnull
    {
        internal Setting(T defaultValue) { Value = defaultValue; Default = defaultValue; }

        /// <summary>
        /// For example,
        /// <code>
        /// MathS.Settings.Precision.As(100, () =>
        /// {
        /// /* some code considering precision = 100 */
        /// });
        /// </code>
        /// </summary>
        /// <param name="value">New value that will be automatically reverted after action is done</param>
        /// <param name="action">What should be done under this setting</param>
        public void As(T value, Action action)
        {
            var previousValue = Value;
            Value = value;
            lock (Value) // TODO: it is probably impossible to access currValue from another thread since it's ThreadStatic
                try
                {
                    action();
                }
                finally
                {
                    Value = previousValue;
                }
        }

        /// <summary>
        /// For example,
        /// <code>
        /// var res = MathS.Settings.Precision.As(100, () =>
        /// {
        ///   /* some code considering precision = 100 */
        ///   return 4;
        /// });
        /// </code>
        /// </summary>
        /// <param name="value">New value that will be automatically reverted after action is done</param>
        /// <param name="action">What should be done under this setting</param>
        public TReturnType As<TReturnType>(T value, Func<TReturnType> action)
        {
            var previousValue = Value;
            Value = value;
            lock (Value) // TODO: it is probably impossible to access currValue from another thread since it's ThreadStatic
                try
                {
                    return action();
                }
                finally
                {
                    Value = previousValue;
                }
        }

        public static implicit operator T(Setting<T> s) => s.Value;
        public static implicit operator Setting<T>(T a) => new(a);
        public override string ToString() => Value.ToString();
        public T Value { get; private set; }
        public T Default { get; }
    }
}

namespace AngouriMath
{
    public static partial class MathS
    {
        public static partial class Settings
        {
            [ThreadStatic]
            private static Setting<bool>? downcastingEnabled;
            [ThreadStatic]
            private static Setting<int>? floatToRationalIterCount;
            [ThreadStatic]
            private static Setting<EInteger>? maxAbsNumeratorOrDenominatorValue;
            [ThreadStatic]
            private static Setting<EDecimal>? precisionErrorCommon;
            [ThreadStatic] 
            private static Setting<EDecimal>? precisionErrorZeroRange;
            [ThreadStatic]
            private static Setting<bool>? allowNewton;
            [ThreadStatic]
            private static Setting<Func<Entity, int>>? complexityCriteria;
            [ThreadStatic]
            private static Setting<NewtonSetting>? newtonSolver;
            [ThreadStatic]
            private static Setting<int>? maxExpansionTermCount;
            [ThreadStatic]
            private static Setting<EContext>? decimalPrecisionContext;
            private static Setting<T> GetCurrentOrDefault<T>(ref Setting<T>? setting, T defaultValue) where T : notnull
            {
                if (setting is null)
                    setting = defaultValue;
                return setting;
            }
        }
    }
}