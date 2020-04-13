using System.Runtime.CompilerServices;
using AngouriMath.Functions.Algebra.AnalyticalSolving;

[assembly: InternalsVisibleTo("UnitTests")]

namespace AngouriMath.Functions.Algebra.Solver.Analytical
{
    /// <summary>
    /// TODO: use on need
    /// </summary>
    internal static class Trigonometry
    {
        /// <summary>
        /// sin(x) = c
        /// x - ?
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        internal static EntitySet FindInvertSin(Entity c)
        {
            var cproots = PolynomialSolver.SolveQuadratic(1, -2 * c * MathS.i, -1);
            return -1 * MathS.i * cproots.Ln();
        }

        /// <summary>
        /// cos(x) = c
        /// x - ?
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        internal static EntitySet FindInvertCos(Entity c)
        {
            var cproots = PolynomialSolver.SolveQuadratic(1, -2 * c, 1);
            return -1 * MathS.i * cproots.Ln();
        }

        /// <summary>
        /// tan(x) = c
        /// x - ?
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        internal static EntitySet FindInvertTan(Entity c)
        {
            var cproots = PolynomialSolver.SolveQuadratic(1, 0, -MathS.Sqrt((c + 1) / (1 - c)));
            return -1 * MathS.i * cproots.Ln();
        }

        /// <summary>
        /// cotan(x) = c
        /// x - ?
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        internal static EntitySet FindInvertCotan(Entity c)
        {
            var cproots = PolynomialSolver.SolveQuadratic(1, 0, -MathS.Sqrt((c + 1) / (c - 1)));
            return -1 * MathS.i * cproots.Ln();
        }
    }
}
