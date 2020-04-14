
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



﻿using System.Runtime.CompilerServices;
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
