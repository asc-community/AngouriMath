
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

namespace AngouriMath
{
    partial record Entity
    {
        partial record Number
        {
            partial record Integer
            {
                private protected override Domain DefaultDomain => Domain.Integer;
            }

            partial record Rational
            {
                private protected override Domain DefaultDomain => Domain.Rational;
            }

            partial record Real
            {
                private protected override Domain DefaultDomain => Domain.Real;
            }

            partial record Complex
            {
                private protected override Domain DefaultDomain => Domain.Complex;
            }
        }

        partial record Sumf
        {
            private protected override Domain DefaultDomain => Domain.Complex;
        }

        partial record Minusf
        {
            private protected override Domain DefaultDomain => Domain.Complex;
        }

        partial record Mulf
        {
            private protected override Domain DefaultDomain => Domain.Complex;
        }

        partial record Divf
        {
            private protected override Domain DefaultDomain => Domain.Complex;
        }

        partial record Sinf
        {
            private protected override Domain DefaultDomain => Domain.Complex;
        }

        partial record Cosf
        {
            private protected override Domain DefaultDomain => Domain.Complex;
        }

        partial record Tanf
        {
            private protected override Domain DefaultDomain => Domain.Complex;
        }

        partial record Cotanf
        {
            private protected override Domain DefaultDomain => Domain.Complex;
        }

        partial record Logf
        {
            private protected override Domain DefaultDomain => Domain.Complex;
        }

        partial record Powf
        {
            private protected override Domain DefaultDomain => Domain.Complex;
        }

        partial record Arcsinf
        {
            private protected override Domain DefaultDomain => Domain.Complex;
        }

        partial record Arccosf
        {
            private protected override Domain DefaultDomain => Domain.Complex;
        }

        partial record Arctanf
        {
            private protected override Domain DefaultDomain => Domain.Complex;
        }

        partial record Arccotanf
        {
            private protected override Domain DefaultDomain => Domain.Complex;
        }

        partial record Factorialf
        {
            private protected override Domain DefaultDomain => Domain.Complex;
        }

        partial record Derivativef
        {
            private protected override Domain DefaultDomain => Domain.Complex;
        }

        partial record Integralf
        {
            private protected override Domain DefaultDomain => Domain.Complex;
        }

        partial record Limitf
        {
            private protected override Domain DefaultDomain => Domain.Complex;
        }

        partial record Signumf
        {
            private protected override Domain DefaultDomain => Domain.Complex;
        }

        partial record Absf
        {
            private protected override Domain DefaultDomain => Domain.Real;
        }

        partial record Boolean
        {
            private protected override Domain DefaultDomain => Domain.Boolean;
        }

        partial record Notf
        {
            private protected override Domain DefaultDomain => Domain.Boolean;
        }

        partial record Andf
        {
            private protected override Domain DefaultDomain => Domain.Boolean;
        }

        partial record Orf
        {
            private protected override Domain DefaultDomain => Domain.Boolean;
        }

        partial record Xorf
        {
            private protected override Domain DefaultDomain => Domain.Boolean;
        }

        partial record Impliesf
        {
            private protected override Domain DefaultDomain => Domain.Boolean;
        }
    }
}
