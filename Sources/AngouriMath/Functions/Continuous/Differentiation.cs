//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using PeterO.Numbers;

namespace AngouriMath
{
    // Adding function Derive to Entity
    partial record Entity
    {
        /// <summary>
        /// Finds the symbolical derivative of the given expression
        /// </summary>
        /// <param name="variable">
        /// Over which variable to find the derivative
        /// </param>
        /// <returns>
        /// The derived expression which might contain <see cref="Derivativef"/> nodes,
        /// or the initial one
        /// </returns>
        /// <example>
        /// <code>
        /// using System;
        /// using static AngouriMath.MathS;
        /// 
        /// var (x, y) = Var("x", "y");
        /// var expr1 = Sqr(x) + Sin(y * x);
        /// Console.WriteLine(expr1);
        /// Console.WriteLine(expr1.Differentiate(x));
        /// Console.WriteLine(expr1.Differentiate(y));
        /// Console.WriteLine(expr1.Differentiate(y).Differentiate(x));
        /// </code>
        /// Prints
        /// <code>
        /// x ^ 2 + sin(y * x)
        /// 2 * x + cos(y * x) * y
        /// cos(y * x) * x
        /// -sin(y * x) * y * x + cos(y * x) 
        /// </code>
        /// </example>
        public Entity Differentiate(Variable variable)
            => InnerDifferentiate(variable).InnerSimplified;

        /// <summary>
        /// Finds the symbolical derivative of the given expression
        /// </summary>
        /// <param name="variable">
        /// Over which variable to find the derivative
        /// </param>
        /// <returns>
        /// The derived expression which might contain <see cref="Derivativef"/> nodes,
        /// or the initial one
        /// </returns>
        [System.Obsolete("Renamed to Differentiate")]
        public Entity Derive(Variable variable)
            => InnerDifferentiate(variable).InnerSimplified;

        /// <summary>
        /// Internal differentiation function
        /// </summary>
        /// <param name="variable">To derive over</param>
        /// <returns>The differentiated expressoin or the Derivative node</returns>
        protected virtual Entity InnerDifferentiate(Variable variable)
            => new Derivativef(this, variable, 1);

        partial record Variable
        {
            
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable) => Name == variable.Name ? 1 : 0;
        }

        partial record Matrix
        {
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable) => Elementwise(e => e.InnerDifferentiate(variable));
        }

        /// <summary>Derives over <paramref name="x"/> <paramref name="power"/> times</summary>
        public Entity Differentiate(Variable x, EInteger power)
        {
            var ent = this;
            for (var _ = 0; _ < power; _++)
                ent = ent.InnerDifferentiate(x);
            return ent;
        }

        partial record Number
        {
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable) => EvalNumerical().IsNaN ? this : 0;
        }

        // Each function and operator processing
        partial record Sumf
        {
            // (a + b)' = a' + b'
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable) =>
                Augend.InnerDifferentiate(variable) + Addend.InnerDifferentiate(variable);
        }

        partial record Minusf
        {
            // (a - b)' = a' - b'
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable) =>
                Subtrahend.InnerDifferentiate(variable) - Minuend.InnerDifferentiate(variable);
        }

        partial record Mulf
        {
            // (a * b)' = a' * b + b' * a
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable) =>
                Multiplier.InnerDifferentiate(variable) * Multiplicand + Multiplicand.InnerDifferentiate(variable) * Multiplier;
        }

        partial record Divf
        {
            // (a / b)' = (a' * b - b' * a) / b^2
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable) =>
                (Dividend.InnerDifferentiate(variable) * Divisor - Divisor.InnerDifferentiate(variable) * Dividend) / Divisor.Pow(2);
        }

        partial record Powf
        {
            // (a ^ b)' = e ^ (ln(a) * b) * (a' * b / a + ln(a) * b')
            // (a ^ const)' = const * a ^ (const - 1) * a'
            // (const ^ b)' = e^b * b'
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable) =>
                Exponent is Number exp
                ? exp * Base.Pow(exp - 1) * Base.InnerDifferentiate(variable)
                : Base is Number
                ? Base.Pow(Exponent) * MathS.Ln(Base) * Exponent.InnerDifferentiate(variable)
                : Base.Pow(Exponent) * (Base.InnerDifferentiate(variable) * Exponent / Base + MathS.Ln(Base) * Exponent.InnerDifferentiate(variable));
        }

        partial record Sinf
        {
            // sin(a)' = cos(a) * a'
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable) =>
                Argument.Cos() * Argument.InnerDifferentiate(variable);
        }

        partial record Cosf
        {
            // cos(a)' = -sin(a) * a'
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable) =>
                -Argument.Sin() * Argument.InnerDifferentiate(variable);
        }

        partial record Tanf
        {
            // tan(a)' = 1 / cos(a) ^ 2 * a'
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable) =>
                1 / Argument.Cos().Pow(2) * Argument.InnerDifferentiate(variable);
        }

        partial record Secantf
        {
            // sec(a)' = sec(a) * tan(a) * a'
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable) =>
                this * Argument.Tan() * Argument.Differentiate(variable);
        }

        partial record Cosecantf
        {
            // csc(a)' = -csc(a) * cotan(a) * a'
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable) =>
                -this * Argument.Cotan() * Argument.Differentiate(variable);
        }

        partial record Arcsecantf
        {
            // asec(a) = 1 / (sqrt(1 - 1 / a2)a2) * a'
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable)
                => 1 / (MathS.Sqrt(1 - 1 / Argument.Pow(2)) * Argument.Pow(2)) * Argument.Differentiate(variable);
        }

        partial record Arccosecantf
        {
            // asec(a) = 1 / (sqrt(1 - 1 / a2)a2) * a'
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable)
                => -1 / (MathS.Sqrt(1 - 1 / Argument.Pow(2)) * Argument.Pow(2)) * Argument.Differentiate(variable);
        }

        partial record Cotanf
        {
            // cot(a)' = -1 / sin(a) ^ 2 * a'
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable) =>
                -1 / Argument.Sin().Pow(2) * Argument.InnerDifferentiate(variable);
        }

        partial record Logf
        {
            // log_b(a)' = (ln(a) / ln(b))' = (ln(a)' * ln(b) - ln(a) * ln(b)') / ln(b)^2 = (a' / a * ln(b) - ln(a) * b' / b) / ln(b)^2
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable) =>
                (Antilogarithm.InnerDifferentiate(variable) / Antilogarithm * MathS.Ln(Base)
                - MathS.Ln(Antilogarithm) * Base.InnerDifferentiate(variable) / Base)
                / MathS.Ln(Base).Pow(2);
        }

        partial record Arcsinf
        {
            // arcsin(x)' = 1 / sqrt(1 - x^2) * x'
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable) =>
                1 / MathS.Sqrt(1 - MathS.Sqr(Argument)) * Argument.InnerDifferentiate(variable);
        }

        partial record Arccosf
        {
            // arccos(x)' = -1 / sqrt(1 - x^2) * x'
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable) =>
                -1 / MathS.Sqrt(1 - MathS.Sqr(Argument)) * Argument.InnerDifferentiate(variable);
        }

        partial record Arctanf
        {
            // arctan(x)' = 1 / (1 + x^2) * x'
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable) =>
                1 / (1 + MathS.Sqr(Argument)) * Argument.InnerDifferentiate(variable);
        }

        partial record Arccotanf
        {
            // arccotan(x)' = -1 / (1 + x^2) * x'
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable) =>
                -1 / (1 + MathS.Sqr(Argument)) * Argument.InnerDifferentiate(variable);
        }

        partial record Factorialf
        {
            // (x!)' = Γ(x + 1) polygamma(0, x + 1)
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable)
            {
                // TODO: Implementation of symbolic gamma function and polygamma functions needed
                return Number.Real.NaN;
            }
        }

#pragma warning disable IDE0054 // Use compound assignment
        partial record Derivativef
        {
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable) =>
                Var == variable
                ? this with { Iterations = Iterations + 1 }
                : MathS.Derivative(this, variable, 1);
        }

        partial record Integralf
        {
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable) =>
                Var == variable
                ? this with { Iterations = Iterations - 1 }
                : MathS.Derivative(this, variable, 1);
        }
#pragma warning restore IDE0054 // Use compound assignment

        partial record Limitf
        {
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable) =>
                // TODO: What is a derivative of a limit?

                // See https://math.stackexchange.com/a/1048570/627798:
                // The derivative itself is a limit, so we can exchange limits if possible. -- Happypig375
                MathS.Derivative(this, variable);
        }

        partial record Signumf
        {
            // TODO: the Delta function required to be defined,
            // or a piecewise definition
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable)
                => MathS.Derivative(this, variable);
        }

        partial record Absf
        {
            // TODO: derivative of the absolute function?
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable)
                => MathS.Signum(Argument) * Argument.InnerDifferentiate(variable);
        }

        partial record Providedf
        {
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable)
                => Expression.InnerDifferentiate(variable).Provided(Predicate);
        }

        partial record Piecewise
        {
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable)
                => New(Cases.Select(c => c.New(c.Expression.InnerDifferentiate(variable), c.Predicate)));
        }

        partial record Application
        {
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable)
                => (Expression, Arguments) switch
                {
                    (Variable v, var args) when args.All(arg => arg.Nodes.Contains(variable) is false)
                        => 0,

                    // d/dx_i f(g_1(x_1, x_2, ..., x_n), g_2(x_1, x_2, ..., x_n), ..., g_n(x_1, x_2, ..., x_n))
                    // becomes
                    // df/d[g_1] * d[g_1] / dx_i + df/d[g_2] * d[g_2] / dx_i + ...
                    // where
                    // f is a short name for f(g_1(...), ...)
                    // g_1 is a short name for g_1(x_1, ...)
                    // etc.
                    //
                    (Variable v, var args) when args.Any(arg => arg.Nodes.Contains(variable))
                        => args.Select(
                            arg =>
                                MathS.Derivative(this, arg) * arg.Differentiate(variable)
                            ).Aggregate((Entity)0, (a, b) => a + b),
                    _ => MathS.Derivative(this, variable)
                };
        }
    }
}