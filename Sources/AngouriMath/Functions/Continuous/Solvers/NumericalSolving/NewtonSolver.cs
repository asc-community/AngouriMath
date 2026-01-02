//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using AngouriMath.Extensions;
using AngouriMath.Functions.Algebra.NumericalSolving;
using PeterO.Numbers;

namespace AngouriMath.Functions.Algebra.NumericalSolving
{
    using static Entity.Number;
    using NumericsComplex = System.Numerics.Complex;
    internal static class NewtonSolver
    {
        /// <summary>Performs a grid search with each iteration done by NewtonIter</summary>
        /// <param name="expr">The equation with one variable to be solved</param>
        /// <param name="v">The variable to solve over</param>
        /// <param name="settings">
        /// Some settings regarding how we should perform the Newton solver process
        /// A complex number, thus, if stepCount.Im == 0, no operations will be performed at all. If you
        /// need to iterate over real numbers only, set it to 1, i. e. new Number(your_number, 1)
        /// How many approximations we need to do before we reach the most precise result.
        /// </param>
        internal static HashSet<Complex> SolveNt(Entity expr, Entity.Variable v, MathS.Settings.NewtonSetting settings)
        {
            // Perform one iteration of searching for a root with Newton-Raphson method
            static Complex NewtonIter(FastExpression f, FastExpression df, NumericsComplex value, int precision)
            {
                var prev = value;

                NumericsComplex ChooseGood() =>
                    NumericsComplex.Abs(prev - value) > (double)MathS.Settings.PrecisionErrorCommon.Value
                    ? double.NaN
                    : value; 

                int minCheckIters = (int)Math.Sqrt(precision);
                for (int i = 0; i < precision; i++)
                {
                    if (i == precision - 1)
                        prev = value;//.Copy();
                    try // TODO: remove try catch in for
                    {

                        var dfv = df.Substitute(value);
                        if (dfv == 0)
                            return ChooseGood();
                        value -= f.Substitute(value) / dfv;
                    }
                    catch (OverflowException)
                    {
                        return ChooseGood();
                    }
                    if (i > minCheckIters && prev == value)
                        return value;
                }
                return ChooseGood();
            }
            if (expr.Vars.Single() != v)
                throw new Core.Exceptions.WrongNumberOfArgumentsException($"{nameof(expr)} should only contain {nameof(Entity.Variable)} {nameof(v)}");

            using var _ = MathS.Settings.FloatToRationalIterCount.Set(0);
            var res = new HashSet<Complex>();
            var df = expr.Differentiate(v).Simplify().Compile(v);
            var f = expr.Simplify().Compile(v);
            for (int x = 0; x < settings.StepCount.Re; x++)
                for (int y = 0; y < settings.StepCount.Im; y++)
                {
                    var xShare = ((EDecimal)x) / settings.StepCount.Re;
                    var yShare = ((EDecimal)y) / settings.StepCount.Im;
                    var value = Complex.Create(
                        settings.From.Re * xShare + settings.To.Re * (1 - xShare),
                        settings.From.Im * yShare + settings.To.Im * (1 - yShare));
                    var root = NewtonIter(f, df, value.ToNumerics(), settings.Precision);
                    if (root.IsFinite && f.Call(root.ToNumerics()).ToNumber().Abs() <
                        MathS.Settings.PrecisionErrorCommon.Value)
                        res.Add(root);
                }
            return res;
        }
    }
}

namespace AngouriMath
{
    partial record Entity : ILatexiseable
    {
        /// <summary>
        /// Searches for numerical solutions via Newton's method
        /// <a href="https://en.wikipedia.org/wiki/Newton%27s_method"/>
        /// To change parameters see <see cref="MathS.Settings.NewtonSolver"/>
        /// </summary>
        public HashSet<Number.Complex> SolveNt(Variable v) =>
            NewtonSolver.SolveNt(this, v, MathS.Settings.NewtonSolver);
    }
}