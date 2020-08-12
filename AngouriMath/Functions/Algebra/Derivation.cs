
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
 using AngouriMath.Core.Sys.Interfaces;
 using PeterO.Numbers;

namespace AngouriMath
{
    // Adding function Derive to Entity
    public abstract partial record Entity : ILatexiseable
    {
        /// <summary>Derivation over a variable (without simplification)</summary>
        /// <param name="x">The variable to derive over</param>
        public abstract Entity Derive(VariableEntity variable);

        /// <summary>Derives over <paramref name="x"/> <paramref name="power"/> times</summary>
        public Entity Derive(VariableEntity x, EInteger power)
        {
            var ent = this;
            for (var _ = 0; _ < power; _++)
                ent = ent.Derive(x);
            return ent;
        }
    }

    public partial record NumberEntity 
    {
        public override Entity Derive(VariableEntity variable) => Eval().IsNaN ? this : 0;
    }
    public partial record VariableEntity 
    {
        public override Entity Derive(VariableEntity variable) => Name == variable.Name ? 1 : 0;
    }
    public partial record Tensor 
    {
        public override Entity Derive(VariableEntity variable) => Elementwise(e => e.Derive(variable));
    }

    // Each function and operator processing
    public partial record Sumf
    {
        // (a + b)' = a' + b'
        public override Entity Derive(VariableEntity variable) =>
            Augend.Derive(variable) + Addend.Derive(variable);
    }
    public partial record Minusf
    {
        // (a - b)' = a' - b'
        public override Entity Derive(VariableEntity variable) =>
            Subtrahend.Derive(variable) - Minuend.Derive(variable);
    }
    public partial record Mulf
    {
        // (a * b)' = a' * b + b' * a
        public override Entity Derive(VariableEntity variable) =>
            Multiplier.Derive(variable) * Multiplicand + Multiplicand.Derive(variable) * Multiplier;
    }

    public partial record Divf
    {
        // (a / b)' = (a' * b - b' * a) / b^2
        public override Entity Derive(VariableEntity variable) =>
            (Dividend.Derive(variable) * Divisor - Divisor.Derive(variable) * Dividend) / Divisor.Pow(2);
    }
    public partial record Powf
    {
        // (a ^ b)' = e ^ (ln(a) * b) * (a' * b / a + ln(a) * b')
        // (a ^ const)' = const * a ^ (const - 1)
        // (const ^ b)' = e^b * b'
        public override Entity Derive(VariableEntity variable) =>
            Exponent is Core.Numerix.ComplexNumber value
            ? Exponent * Base.Pow(value - 1) * Base.Derive(variable)
            : Base is NumberEntity
            ? Base.Pow(Exponent) * MathS.Ln(Base) * Exponent.Derive(variable)
            : Base.Pow(Exponent) * (Base.Derive(variable) * Exponent / Base + MathS.Ln(Base) * Exponent.Derive(variable));
    }
    public partial record Sinf
    {
        // sin(a)' = cos(a) * a'
        public override Entity Derive(VariableEntity variable) =>
            Argument.Cos() * Argument.Derive(variable);
    }
    public partial record Cosf
    {
        // cos(a)' = -sin(a) * a'
        public override Entity Derive(VariableEntity variable) =>
            -1 * Argument.Sin() * Argument.Derive(variable);
    }
    public partial record Tanf
    {
        // tan(a)' = 1 / cos(a) ^ 2 * a'
        public override Entity Derive(VariableEntity variable) =>
            1 / Argument.Cos().Pow(2) * Argument.Derive(variable);
    }
    public partial record Cotanf
    {
        // cot(a)' = -1 / sin(a) ^ 2 * a'
        public override Entity Derive(VariableEntity variable) =>
            -1 / Argument.Sin().Pow(2) * Argument.Derive(variable);
    }
    public partial record Logf
    {
        // log_b(a)' = (ln(a) / ln(b))' = (ln(a)' * ln(b) - ln(a) * ln(b)') / ln(b)^2 = (a' / a * ln(b) - ln(a) * b' / b) / ln(b)^2
        public override Entity Derive(VariableEntity variable) =>
            (Antilogarithm.Derive(variable) / Antilogarithm * MathS.Ln(Base)
            - MathS.Ln(Antilogarithm) * Base.Derive(variable) / Base)
            / MathS.Ln(Base).Pow(2);
    }
    public partial record Arcsinf
    {
        // arcsin(x)' = 1 / sqrt(1 - x^2) * x'
        public override Entity Derive(VariableEntity variable) =>
            1 / MathS.Sqrt(1 - MathS.Sqr(Argument)) * Argument.Derive(variable);
    }
    public partial record Arccosf
    {
        // arccos(x)' = -1 / sqrt(1 - x^2) * x'
        public override Entity Derive(VariableEntity variable) =>
            -1 / MathS.Sqrt(1 - MathS.Sqr(Argument)) * Argument.Derive(variable);
    }
    public partial record Arctanf
    {
        // arctan(x)' = 1 / (1 + x^2) * x'
        public override Entity Derive(VariableEntity variable) =>
            1 / (1 + MathS.Sqr(Argument)) * Argument.Derive(variable);
    }
    public partial record Arccotanf
    {
        // arccotan(x)' = -1 / (1 + x^2) * x'
        public override Entity Derive(VariableEntity variable) =>
            -1 / (1 + MathS.Sqr(Argument)) * Argument.Derive(variable);
    }
    public partial record Factorialf
    {
        // (x!)' = Γ(x + 1) polygamma(0, x + 1)
        public override Entity Derive(VariableEntity variable)
        {
            // TODO: Implementation of symbolic gamma function and polygamma functions needed
            return Core.Numerix.RealNumber.NaN;
        }
    }

    public partial record Derivativef
    {
        public override Entity Derive(VariableEntity variable) =>
            Variable == variable
            ? this with { Iterations = Iterations + 1 }
            : MathS.Derivative(this, variable, 1);
    }

    public partial record Integralf
    {
        public override Entity Derive(VariableEntity variable) =>
            Variable == variable
            ? this with { Iterations = Iterations - 1 }
            : MathS.Derivative(this, variable, 1);
    }

    public partial record Limitf
    {
        public override Entity Derive(VariableEntity variable) =>
            // TODO: What is a derivative of a limit?

            // See https://math.stackexchange.com/a/1048570/627798:
            // The derivative itself is a limit, so we can exchange limits if possible. -- Happypig375
            MathS.Derivative(this, variable);
    }
}
