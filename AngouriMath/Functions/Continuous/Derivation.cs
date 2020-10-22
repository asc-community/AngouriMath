/*
 * Copyright (c) 2019-2020 Angourisoft
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using PeterO.Numbers;

namespace AngouriMath
{
    // Adding function Derive to Entity
    public abstract partial record Entity
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

        public partial record Variable
        {
            
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable) => Name == variable.Name ? 1 : 0;
        }

        public partial record Tensor
        {
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable) => Elementwise(e => e.InnerDifferentiate(variable));
        }

        /// <summary>Derives over <paramref name="x"/> <paramref name="power"/> times</summary>
        public Entity Derive(Variable x, EInteger power)
        {
            var ent = this;
            for (var _ = 0; _ < power; _++)
                ent = ent.InnerDifferentiate(x);
            return ent;
        }

        public partial record Number
        {
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable) => EvalNumerical().IsNaN ? this : 0;
        }

        // Each function and operator processing
        public partial record Sumf
        {
            // (a + b)' = a' + b'
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable) =>
                Augend.InnerDifferentiate(variable) + Addend.InnerDifferentiate(variable);
        }

        public partial record Minusf
        {
            // (a - b)' = a' - b'
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable) =>
                Subtrahend.InnerDifferentiate(variable) - Minuend.InnerDifferentiate(variable);
        }

        public partial record Mulf
        {
            // (a * b)' = a' * b + b' * a
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable) =>
                Multiplier.InnerDifferentiate(variable) * Multiplicand + Multiplicand.InnerDifferentiate(variable) * Multiplier;
        }

        public partial record Divf
        {
            // (a / b)' = (a' * b - b' * a) / b^2
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable) =>
                (Dividend.InnerDifferentiate(variable) * Divisor - Divisor.InnerDifferentiate(variable) * Dividend) / Divisor.Pow(2);
        }

        public partial record Powf
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

        public partial record Sinf
        {
            // sin(a)' = cos(a) * a'
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable) =>
                Argument.Cos() * Argument.InnerDifferentiate(variable);
        }

        public partial record Cosf
        {
            // cos(a)' = -sin(a) * a'
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable) =>
                -Argument.Sin() * Argument.InnerDifferentiate(variable);
        }

        public partial record Tanf
        {
            // tan(a)' = 1 / cos(a) ^ 2 * a'
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable) =>
                1 / Argument.Cos().Pow(2) * Argument.InnerDifferentiate(variable);
        }

        public partial record Cotanf
        {
            // cot(a)' = -1 / sin(a) ^ 2 * a'
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable) =>
                -1 / Argument.Sin().Pow(2) * Argument.InnerDifferentiate(variable);
        }

        public partial record Logf
        {
            // log_b(a)' = (ln(a) / ln(b))' = (ln(a)' * ln(b) - ln(a) * ln(b)') / ln(b)^2 = (a' / a * ln(b) - ln(a) * b' / b) / ln(b)^2
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable) =>
                (Antilogarithm.InnerDifferentiate(variable) / Antilogarithm * MathS.Ln(Base)
                - MathS.Ln(Antilogarithm) * Base.InnerDifferentiate(variable) / Base)
                / MathS.Ln(Base).Pow(2);
        }

        public partial record Arcsinf
        {
            // arcsin(x)' = 1 / sqrt(1 - x^2) * x'
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable) =>
                1 / MathS.Sqrt(1 - MathS.Sqr(Argument)) * Argument.InnerDifferentiate(variable);
        }

        public partial record Arccosf
        {
            // arccos(x)' = -1 / sqrt(1 - x^2) * x'
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable) =>
                -1 / MathS.Sqrt(1 - MathS.Sqr(Argument)) * Argument.InnerDifferentiate(variable);
        }

        public partial record Arctanf
        {
            // arctan(x)' = 1 / (1 + x^2) * x'
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable) =>
                1 / (1 + MathS.Sqr(Argument)) * Argument.InnerDifferentiate(variable);
        }

        public partial record Arccotanf
        {
            // arccotan(x)' = -1 / (1 + x^2) * x'
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable) =>
                -1 / (1 + MathS.Sqr(Argument)) * Argument.InnerDifferentiate(variable);
        }

        public partial record Factorialf
        {
            // (x!)' = Γ(x + 1) polygamma(0, x + 1)
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable)
            {
                // TODO: Implementation of symbolic gamma function and polygamma functions needed
                return Number.Real.NaN;
            }
        }

        public partial record Derivativef
        {
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable) =>
                Var == variable
                ? this with { Iterations = Iterations + 1 }
                : MathS.Derivative(this, variable, 1);
        }

        public partial record Integralf
        {
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable) =>
                Var == variable
                ? this with { Iterations = Iterations - 1 }
                : MathS.Derivative(this, variable, 1);
        }

        public partial record Limitf
        {
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable) =>
                // TODO: What is a derivative of a limit?

                // See https://math.stackexchange.com/a/1048570/627798:
                // The derivative itself is a limit, so we can exchange limits if possible. -- Happypig375
                MathS.Derivative(this, variable);
        }

        public partial record Signumf
        {
            // TODO: the Delta function required to be defined,
            // or a piecewise definition
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable)
                => MathS.Derivative(this, variable);
        }

        public partial record Absf
        {
            // TODO: derivative of the absolute function?
            /// <inheritdoc/>
            protected override Entity InnerDifferentiate(Variable variable)
                => MathS.Signum(Argument) * Argument.InnerDifferentiate(variable);
        }
    }
}