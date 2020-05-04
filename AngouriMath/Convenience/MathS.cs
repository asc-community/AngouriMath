
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
using AngouriMath.Core;
using AngouriMath.Core.FromString;
using System.Linq.Expressions;
using AngouriMath.Core.FromLinq;
using AngouriMath.Core.TreeAnalysis;
using AngouriMath.Functions.NumberSystem;
using AngouriMath.Functions.Output;
using AngouriMath.Functions.Algebra.Solver;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
 using AngouriMath.Core.Sys;
 using AngouriMath.Core.Sys.Interfaces;
using AngouriMath.Functions.Algebra.AnalyticalSolving;

namespace AngouriMath
{
    /// <summary>
    /// Use functions from this class
    /// </summary>
    public static partial class MathS
    {
        /// <summary>
        /// Use it to solve equations
        /// </summary>
        /// <param name="equations"></param>
        /// <returns></returns>
        public static EquationSystem Equations(params Entity[] equations)
        => new EquationSystem(equations);
        

        /// <summary>
        /// Solves one equation over one variable
        /// </summary>
        /// <param name="equation"></param>
        /// <param name="var"></param>
        /// <returns></returns>
        public static Set SolveEquation(Entity equation, VariableEntity var)
            => EquationSolver.Solve(equation, var);

        /// <summary>
        /// https://en.wikipedia.org/wiki/Trigonometric_functions
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// A / C
        /// </returns>
        public static Entity Sin(Entity a) => Sinf.Hang(a);

        /// <summary>
        /// https://en.wikipedia.org/wiki/Trigonometric_functions
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// B / C
        /// </returns>
        public static Entity Cos(Entity a) => Cosf.Hang(a);

        /// <summary>
        /// https://en.wikipedia.org/wiki/Logarithm
        /// </summary>
        /// <param name="num"></param>
        /// <param name="base_"></param>
        /// <returns></returns>
        public static Entity Log(Entity num, Entity base_) => Logf.Hang(num, base_);

        /// <summary>
        /// https://en.wikipedia.org/wiki/Power_function
        /// </summary>
        /// <param name="base_"></param>
        /// <param name="power"></param>
        /// <returns></returns>
        public static Entity Pow(Entity base_, Entity power) => Powf.Hang(base_, power);

        /// <summary>
        /// Special case of power function
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// a ^ 0.5
        /// </returns>
        public static Entity Sqrt(Entity a) => Powf.Hang(a, 0.5);

        /// <summary>
        /// Special case of power function
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// a ^ 2
        /// </returns>
        public static Entity Sqr(Entity a) => Powf.Hang(a, 2);

        /// <summary>
        /// https://en.wikipedia.org/wiki/Trigonometric_functions
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// A / B
        /// </returns>
        public static Entity Tan(Entity a) => Tanf.Hang(a);

        /// <summary>
        /// https://en.wikipedia.org/wiki/Trigonometric_functions
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// B / A
        /// </returns>
        public static Entity Cotan(Entity a) => Cotanf.Hang(a);

        /// <summary>
        /// https://en.wikipedia.org/wiki/Trigonometric_functions
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// C / B
        /// </returns>
        public static Entity Sec(Entity a) => 1 / Cos(a);

        /// <summary>
        /// https://en.wikipedia.org/wiki/Trigonometric_functions
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// C / A
        /// </returns>
        public static Entity Cosec(Entity a) => 1 / Sin(a);

        /// <summary>
        /// This function is every interesting for ASC (https://asc-community.org)
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// x * sin(x)
        /// </returns>
        public static Entity B(Entity a) => a * Sin(a);

        /// <summary>
        /// This function is every interesting for ASC (https://asc-community.org)
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// x * cos(x)
        /// </returns>
        public static Entity TB(Entity a) => a * Cos(a);

        /// <summary>
        /// https://en.wikipedia.org/wiki/Inverse_trigonometric_functions
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// angle between A and C
        /// </returns>
        public static Entity Arcsin(Entity a) => Arcsinf.Hang(a);

        /// <summary>
        /// https://en.wikipedia.org/wiki/Inverse_trigonometric_functions
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// angle between B and C
        /// </returns>
        public static Entity Arccos(Entity a) => Arccosf.Hang(a);

        /// <summary>
        /// https://en.wikipedia.org/wiki/Inverse_trigonometric_functions
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// angle between A and C
        /// </returns>
        public static Entity Arctan(Entity a) => Arctanf.Hang(a);

        /// <summary>
        /// https://en.wikipedia.org/wiki/Inverse_trigonometric_functions
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// angle between B and A
        /// </returns>
        public static Entity Arccotan(Entity a) => Arccotanf.Hang(a);
     
        /// <summary>
        /// https://en.wikipedia.org/wiki/Inverse_trigonometric_functions
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// angle between C and B
        /// </returns>
        public static Entity Arcsec(Entity a) => Arccosf.Hang(1 / a);
     
        /// <summary>
        /// https://en.wikipedia.org/wiki/Inverse_trigonometric_functions
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// angle between C and A
        /// </returns>
        public static Entity Arccosec(Entity a) => Arcsinf.Hang(1 / a);

        /// <summary>
        /// https://en.wikipedia.org/wiki/Natural_logarithm
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Entity Ln(Entity a) => Logf.Hang(a, e);

        /// <summary>
        /// Creates an instance of variable entity.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static VariableEntity Var(string name) => new VariableEntity(name);

        /// <summary>
        /// Creates a complex instance of Number (not NumberEntity!)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Number Num(double a, double b) => new Number(a, b);

        /// <summary>
        /// Creates a real instance of Number (not NumberEntity!)
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Number Num(double a) => new Number(a);

        /// <summary>
        /// List of public constants
        /// </summary>
        public static readonly VariableEntity e = "e";
        public static readonly Number i = new Number(0, 1);
        public static readonly VariableEntity pi = "pi";

        /// <summary>
        /// Converts an expression from a string
        /// </summary>
        /// <param name="expr">
        /// String expression, for example, "2 * x + 3 + sqrt(x)"
        /// </param>
        /// <returns></returns>
        public static Entity FromString(string expr) => FromString(expr, true);

        /// <summary>
        /// Converts an expression from a string
        /// </summary>
        /// <param name="expr">
        /// String expression, for example, "2 * x + 3 + sqrt(x)"
        /// </param>
        /// <param name="intelli">
        /// Bool parameter responsible for neat-syntax parsing, for example
        /// 2x will be parsed as 2 * x.
        /// </param>
        /// <returns></returns>
        public static Entity FromString(string expr, bool intelli)
            => Parser.Parse(expr);

        /// <summary>
        /// Returns list of unique variables, for example 
        /// it extracts `x`, `goose` from (x + 2 * goose) - pi * x
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        [ObsoleteAttribute("Use MathS.Utils.GetUniqueVariables instead")]
        public static Set GetUniqueVariables(Entity expr)
        {
            var res = new Set();
            TreeAnalyzer.GetUniqueVariables(expr, res);
            return res;
        }

        /// <summary>
        /// Translates num10 into another number system
        /// </summary>
        /// <param name="num"></param>
        /// <param name="N"></param>
        /// <returns></returns>
        public static string ToBaseN(double num, int N) => NumberSystem.ToBaseN(num, N);

        /// <summary>
        /// Translates num into 10 number system
        /// </summary>
        /// <param name="num"></param>
        /// <param name="N"></param>
        /// <returns></returns>
        public static double FromBaseN(string num, int N) => NumberSystem.FromBaseN(num, N);

        
        
        /// <summary>
        /// Returns LaTeX code of the argument
        /// </summary>
        /// <param name="latexiseable"></param>
        /// <returns></returns>
        public static string Latex(ILatexiseable latexiseable)
            => latexiseable.Latexise();


        /// <summary>
        /// Classes and functions related to matrices defined here
        /// </summary>
        public static class Matrices
        {
            /// <summary>
            /// Creates an instance of Tensor: Matrix
            /// Usage example:
            /// var t = MathS.Matrix(5, 3,
            ///        10, 11, 12,
            ///        20, 21, 22,
            ///        30, 31, 32,
            ///        40, 41, 42,
            ///        50, 51, 52
            ///        );
            /// creates matrix 5x3 with the appropriate elements
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            public static Tensor Matrix(int rows, int columns, params Entity[] values)
            {
                if (values.Length != rows * columns)
                    throw new MathSException("Axes don't match data");
                var r = new Tensor(rows, columns);
                r.Assign(values);
                return r;
            }

            /// <summary>
            /// Creates an instance of vector
            /// </summary>
            /// <param name="p"></param>
            /// <returns></returns>
            public static Tensor Vector(params Entity[] p)
            {
                var r = new Tensor(p.Length);
                r.Assign(p);
                return r;
            }

            /// <summary>
            /// Returns dot product of two matrices
            /// </summary>
            /// <param name="A"></param>
            /// <param name="B"></param>
            /// <returns></returns>
            public static Tensor DotProduct(Tensor A, Tensor B) =>
                AngouriMath.Core.Sys.Items.Tensors.TensorFunctional.DotProduct(A, B);

            /// <summary>
            /// Returns scalar product of two matrices
            /// </summary>
            /// <param name="A"></param>
            /// <param name="B"></param>
            /// <returns></returns>
            public static Entity ScalarProduct(Tensor A, Tensor B) =>
                AngouriMath.Core.Sys.Items.Tensors.TensorFunctional.ScalarProduct(A, B);

        }

        /// <summary>
        /// Some additional functions defined here
        /// </summary>
        public static class Utils
        {
            /// <summary>
            /// Returns an entity in polynomial order if possible
            /// </summary>
            /// <param name="expr">
            /// To parse from
            /// </param>
            /// <param name="variable">
            /// Polynomial is a function of a variable
            /// </param>
            /// <param name="dst">
            /// To return to
            /// </param>
            /// <returns>
            /// true if success
            /// false otherwise (do not access dst in this case, it's undefined)
            /// </returns>
            public static bool TryPolynomial(Entity expr, VariableEntity variable, out Entity dst)
                => Functions.Utils.TryPolynomial(expr, variable, out dst);

            /// <summary>
            /// Sets threshold for comparison
            /// For example, if you don't need precision higher than 6 digits after .,
            /// you can set it to 1.0e-6 so 1.0000000 == 0.9999999
            /// </summary>
            public static double EQUALITY_THRESHOLD { get; set; } = 1.0e-7;

            /// <summary>
            /// Converts an exprssion from linq expression
            /// </summary>
            /// <param name="expr"></param>
            /// <returns></returns>
            public static Entity FromLinq(Expression expr)
            {
                var parser = new LinqParser(expr);
                return parser.Parse();
            }

            /// <summary>
            /// Checks tree for some unexpected bad occasions
            /// Throws SysException's children
            /// If you need a message, it's better to write
            /// try
            /// {
            ///     MathS.CheckTree(a);
            /// }
            /// catch (SysException e)
            /// {
            ///     Console.WriteLine(e.Message);
            /// }
            /// </summary>
            /// <param name="expr"></param>
            /// <returns></returns>
            public static void CheckTree(Entity expr) => TreeAnalyzer.CheckTree(expr);

            /// <summary>
            /// Optimizes tree to binary
            /// Might boost some operations
            /// Not necessary to use
            /// </summary>
            /// <param name="tree"></param>
            /// <returns></returns>
            public static Entity OptimizeTree(Entity tree)
            {
                tree = tree.DeepCopy();
                TreeAnalyzer.Optimization.OptimizeTree(ref tree);
                return tree;
            }

            /// <summary>
            /// Returns sympy interpretable format
            /// </summary>
            /// <param name="expr"></param>
            /// <returns></returns>
            public static string ToSympyCode(Entity expr) => Functions.Output.ToSympy.GenerateCode(expr);

            /// <summary>
            /// Returns list of unique variables, for example 
            /// it extracts `x`, `goose` from (x + 2 * goose) - pi * x
            /// </summary>
            /// <param name="expr"></param>
            /// <returns></returns>
            public static Set GetUniqueVariables(Entity expr)
            {
                var res = new Set();
                TreeAnalyzer.GetUniqueVariables(expr, res);
                return res;
            }
        }

        /// <summary>
        /// Functions and classes related to sets defined here
        /// 
        /// Class SetNode defines true mathematical sets
        /// It can be empty, it can contain numbers, it can contain intervals etc.
        /// It supports intersection (with & operator), union (with | operator), subtracting (with - operator)
        /// </summary>
        public static class Sets
        {
            /// <summary>
            /// Creates an instance of an empty set
            /// </summary>
            /// <returns></returns>
            public static Set Empty()
                => new Set();

            /// <summary>
            /// Returns a set of all complex numbers
            /// </summary>
            /// <returns></returns>
            public static Set C()
                => new Set
                {
                    Pieces = new List<Piece>
                    {
                        Piece.Interval(
                            new Number(double.NegativeInfinity, double.NegativeInfinity),
                            new Number(double.PositiveInfinity, double.PositiveInfinity)
                            ).AsInterval().SetLeftClosed(false, false).SetRightClosed(false, false)
                    }
                };

            /// <summary>
            /// Returns a set of all real numbers
            /// </summary>
            /// <returns></returns>
            public static Set R()
                => new Set
                {
                    Pieces = new List<Piece>
                    {
                        Piece.Interval(
                            new Number(double.NegativeInfinity),
                            new Number(double.PositiveInfinity)
                        ).AsInterval().SetLeftClosed(false).SetRightClosed(false)
                    }
                };

            /// <summary>
            /// Creats a set that you can fill with elements
            /// Later on, you may add an Interval if wish
            /// </summary>
            /// <param name="entities"></param>
            /// <returns></returns>
            public static Set Finite(params Entity[] entities)
            {
                var res = new Set();
                foreach (var entity in entities)
                    res.AddElements(entity);
                return res;
            }

            /// <summary>
            /// Creates an interval
            /// To modify it, use
            /// Interval(3, 4).SetLeftClosed (see more alike functions in set documentation)
            /// </summary>
            /// <param name="from"></param>
            /// <param name="to"></param>
            /// <returns></returns>
            public static IntervalPiece Interval(Entity from, Entity to)
                => Piece.Interval(from, to).AsInterval();
        }

    }
}


////////////////////////////////////////////////////////////////////////////////////
/*
 *
 * Obsolete attributes. Not recommended to use
 *
 */
////////////////////////////////////////////////////////////////////////////////////


namespace AngouriMath
{
    public static partial class MathS
    {
        /// <summary>
        /// Sets threshold for comparison
        /// For example, if you don't need precision higher than 6 digits after .,
        /// you can set it to 1.0e-6 so 1.0000000 == 0.9999999
        /// </summary>
        [Obsolete("Use MathS.Utils.EQUALITY_THRESHOLD instead")]
        public static double EQUALITY_THRESHOLD { get; set; } = 1.0e-7;

        /// <summary>
        /// Converts an exprssion from linq expression
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        [Obsolete("Use MathS.Utils.FromLinq instead")]
        public static Entity FromLinq(Expression expr)
        {
            var parser = new LinqParser(expr);
            return parser.Parse();
        }

        /// <summary>
        /// Checks tree for some unexpected bad occasions
        /// Throws SysException's children
        /// If you need a message, it's better to write
        /// try
        /// {
        ///     MathS.CheckTree(a);
        /// }
        /// catch (SysException e)
        /// {
        ///     Console.WriteLine(e.Message);
        /// }
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        [Obsolete("Use MathS.Utils.CheckTree instead")]
        public static void CheckTree(Entity expr) => TreeAnalyzer.CheckTree(expr);

        /// <summary>
        /// Returns sympy interpretable format
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        [Obsolete("Use MathS.Utils.ToSympyCode instead")]
        public static string ToSympyCode(Entity expr) => Functions.Output.ToSympy.GenerateCode(expr);


        /// <summary>
        /// Optimizes tree to binary
        /// Might boost some operations
        /// Not necessary to use
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        [Obsolete("Use MathS.Utils.OptimizeTree instead")]
        public static Entity OptimizeTree(Entity tree)
        {
            tree = tree.DeepCopy();
            TreeAnalyzer.Optimization.OptimizeTree(ref tree);
            return tree;
        }

        /// <summary>
        /// Creates an instance of Tensor: Matrix
        /// Usage example:
        /// var t = MathS.Matrix(5, 3,
        ///        10, 11, 12,
        ///        20, 21, 22,
        ///        30, 31, 32,
        ///        40, 41, 42,
        ///        50, 51, 52
        ///        );
        /// creates matrix 5x3 with the appropriate elements
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        [Obsolete("Use MathS.Matrices.Matrix instead")]
        public static Tensor Matrix(int rows, int columns, params Entity[] values)
        {
            if (values.Length != rows * columns)
                throw new MathSException("Axes don't match data");
            var r = new Tensor(rows, columns);
            r.Assign(values);
            return r;
        }

        /// <summary>
        /// Creates an instance of vector
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        [Obsolete("Use MathS.Matrices.Vector instead")]
        public static Tensor Vector(params Entity[] p)
        {
            var r = new Tensor(p.Length);
            r.Assign(p);
            return r;
        }

        /// <summary>
        /// Returns dot product of two matrices
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        [Obsolete("Use MathS.Matrices.DotProduct instead")]
        public static Tensor DotProduct(Tensor A, Tensor B) =>
            AngouriMath.Core.Sys.Items.Tensors.TensorFunctional.DotProduct(A, B);

        /// <summary>
        /// Returns scalar product of two matrices
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        [Obsolete("Use MathS.Matrices.ScalarProduct instead")]
        public static Entity ScalarProduct(Tensor A, Tensor B) =>
            AngouriMath.Core.Sys.Items.Tensors.TensorFunctional.ScalarProduct(A, B);

        /// <summary>
        /// Solves a system of equations
        /// </summary>
        /// <param name="equations"></param>
        /// <param name="vars"></param>
        /// <returns>
        /// Returns a matrix of solutions
        /// matrix.shape[0] - number of solutions
        /// matrix.shape[1] is equal to amount of variables
        /// </returns>
        [Obsolete("Use MathS.Equations instead")]
        public static Tensor Solve(List<Entity> equations, List<VariableEntity> vars)
            => EquationSolver.SolveSystem(equations, vars);

        /// <summary>
        /// Solves one equation over one variable
        /// </summary>
        /// <param name="equation"></param>
        /// <param name="var"></param>
        /// <returns></returns>
        [Obsolete("Use either MathS.Equations or MathS.SolveEquation or yourexpr.SolveEquation instead")]
        public static Set Solve(Entity equation, VariableEntity var)
            => EquationSolver.Solve(equation, var);
    }
}