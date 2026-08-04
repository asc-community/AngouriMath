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
            var df = WithoutConditions(expr.Differentiate(v).Simplify()).Compile(v);
            var f = WithoutConditions(expr.Simplify()).Compile(v);
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
            // Rounded and collapsed first, then verified: both cut down the duplicates the
            // grid search produces, so there is less to verify, and what gets verified is
            // the values actually handed back.
            var distinct = OnePerRoot(WithoutIterationNoise(res), f);
            distinct.RemoveWhere(root => !Satisfies(expr, v, root));
            return distinct;
        }

        /// <summary>
        /// How far apart two candidates may be, relative to their own size, and still be
        /// the same root.
        /// </summary>
        /// <remarks>
        /// The search starts from a grid and iterates in double precision, so the same root
        /// reached from different starting points comes back agreeing to about sixteen
        /// significant digits and differing after that. Left as they were, those were
        /// counted as separate answers: x^5 + 3x + 1 came back with 28 roots and x^6 + x + 1
        /// with 23, four of the 28 being -0.83907243306660750, -0.83907243306660761,
        /// -0.83907243306660773 and -0.83907243306660784.
        ///
        /// The measured room is wide. Candidates for one root lie within 1e-15 of each other
        /// relative to their size, while the nearest two distinct roots over the twelve
        /// polynomials tried are 0.42 apart. This is 1e-13: a hundred times above the noise
        /// and still twelve orders below the closest two roots that were ever told apart.
        /// </remarks>
        [ConstantField] private static readonly EDecimal SameRootTolerance = EDecimal.Create(1, -13);

        private static bool AreTheSameRoot(Complex a, Complex b)
        {
            var apart = EDecimal.Max(
                a.RealPart.EDecimal.Subtract(b.RealPart.EDecimal).Abs(),
                a.ImaginaryPart.EDecimal.Subtract(b.ImaginaryPart.EDecimal).Abs());
            var size = EDecimal.Max(EDecimal.One, EDecimal.Max(
                a.RealPart.EDecimal.Abs(), a.ImaginaryPart.EDecimal.Abs()));
            return !apart.GreaterThan(size.Multiply(SameRootTolerance));
        }

        /// <summary>
        /// One candidate per root, keeping whichever of each group leaves the equation
        /// closest to zero. Ordered first, so that which candidate is compared against
        /// which does not depend on the order the grid happened to produce them in.
        /// </summary>
        private static HashSet<Complex> OnePerRoot(HashSet<Complex> roots, FastExpression f)
        {
            EDecimal Residual(Complex root)
            {
                try { return f.Call(root.ToNumerics()).ToNumber().Abs().EDecimal; }
                catch (System.Exception) { return EDecimal.PositiveInfinity; }
            }
            var kept = new List<Complex>();
            foreach (var root in roots
                .OrderBy(root => root.RealPart.EDecimal)
                .ThenBy(root => root.ImaginaryPart.EDecimal))
            {
                var same = kept.FindIndex(other => AreTheSameRoot(other, root));
                if (same < 0)
                    kept.Add(root);
                else if (Residual(root).CompareTo(Residual(kept[same])) < 0)
                    kept[same] = root;
            }
            return new HashSet<Complex>(kept);
        }

        /// <summary>
        /// Rounds away the part of each root that the iteration cannot have got right.
        /// </summary>
        /// <remarks>
        /// The iteration works in <see cref="System.Numerics.Complex"/>, so a
        /// component below roughly 1e-15 of the root is noise from the iteration and not
        /// part of the answer. It matters because the search starts from a grid: the same
        /// root reached from different starting points differs in those last digits, so
        /// left in, the one real root of <c>x^5 + 3x + 1</c> comes back as four different
        /// complex numbers with imaginary parts around 1e-19. Rounded, they are one root
        /// again, and the set collapses them.
        /// </remarks>
        private static HashSet<Complex> WithoutIterationNoise(HashSet<Complex> roots)
        {
            using var _ = MathS.Settings.PrecisionErrorZeroRange.Set(EDecimal.Create(1, -15));
            return new HashSet<Complex>(roots.Select(root =>
                Complex.Create(root.RealPart.EDecimal, root.ImaginaryPart.EDecimal)));
        }

        /// <summary>
        /// Strips the domain conditions off an expression. Simplification leaves them
        /// behind -- log(a) + log(b) only collapses to log(a * b) where both are positive,
        /// for one -- and the compiler has no way to represent an <see cref="Entity.Providedf"/>,
        /// so <c>x + ln(x) = 0</c> used to come out of the public solver as an
        /// <see cref="Core.Exceptions.UncompilableNodeException"/>. Newton iterates on the
        /// unconditioned expression and <see cref="Satisfies"/> re-imposes the conditions.
        /// </summary>
        private static Entity WithoutConditions(Entity expr)
            => expr.Replace(node => node is Entity.Providedf(var inner, _) ? inner : node);

        /// <summary>
        /// Whether a candidate root really is one, checked against the expression as it
        /// was handed in rather than against the simplified form Newton iterated on.
        /// Simplification can widen the domain: <c>ln(x) + ln(x+1)</c> becomes
        /// <c>ln(x * (x+1))</c>, whose root -1.618... leaves the original at 2*pi*i, and
        /// the solver reported it as a root of the original equation.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> for anything that cannot be checked, so that a root is
        /// only ever dropped on positive evidence that it is spurious.
        /// </returns>
        private static bool Satisfies(Entity expr, Entity.Variable v, Complex root)
        {
            try
            {
                return expr.Substitute(v, root).Evaled is not Complex residual
                    || !residual.IsFinite
                    || residual.Abs().EDecimal.LessThan(MathS.Settings.PrecisionErrorCommon);
            }
            catch (Core.Exceptions.AngouriBugException) { throw; }
            catch (System.Exception) { return true; }
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