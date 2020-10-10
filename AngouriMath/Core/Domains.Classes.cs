
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

using AngouriMath.Core;

namespace AngouriMath
{
    partial record Entity
    {
        partial record Variable
        {
            public override Domain Codomain { get; protected init; } = Domain.Any;
        }

        partial record Tensor
        {
            public override Domain Codomain { get; protected init; } = Domain.Any;
        }

        partial record Sumf
        {
            public override Domain Codomain { get; protected init; } = Domain.Complex;
        }

        partial record Minusf
        {
            public override Domain Codomain { get; protected init; } = Domain.Complex;
        }

        partial record Mulf
        {
            public override Domain Codomain { get; protected init; } = Domain.Complex;
        }

        partial record Divf
        {
            public override Domain Codomain { get; protected init; } = Domain.Complex;
        }

        partial record Sinf
        {
            public override Domain Codomain { get; protected init; } = Domain.Complex;
        }

        partial record Cosf
        {
            public override Domain Codomain { get; protected init; } = Domain.Complex;
        }

        partial record Tanf
        {
            public override Domain Codomain { get; protected init; } = Domain.Complex;
        }

        partial record Cotanf
        {
            public override Domain Codomain { get; protected init; } = Domain.Complex;
        }

        partial record Logf
        {
            public override Domain Codomain { get; protected init; } = Domain.Complex;
        }

        partial record Powf
        {
            public override Domain Codomain { get; protected init; } = Domain.Complex;
        }

        partial record Arcsinf
        {
            public override Domain Codomain { get; protected init; } = Domain.Complex;
        }

        partial record Arccosf
        {
            public override Domain Codomain { get; protected init; } = Domain.Complex;
        }

        partial record Arctanf
        {
            public override Domain Codomain { get; protected init; } = Domain.Complex;
        }

        partial record Arccotanf
        {
            public override Domain Codomain { get; protected init; } = Domain.Complex;
        }

        partial record Factorialf
        {
            public override Domain Codomain { get; protected init; } = Domain.Complex;
        }

        partial record Derivativef
        {
            public override Domain Codomain { get; protected init; } = Domain.Complex;
        }

        partial record Integralf
        {
            public override Domain Codomain { get; protected init; } = Domain.Complex;
        }

        partial record Limitf
        {
            public override Domain Codomain { get; protected init; } = Domain.Complex;
        }

        partial record Signumf
        {
            public override Domain Codomain { get; protected init; } = Domain.Complex;
        }

        partial record Absf
        {
            public override Domain Codomain { get; protected init; } = Domain.Real;
        }

        partial record Boolean
        {
            public override Domain Codomain { get; protected init; } = Domain.Boolean;
        }

        partial record Notf
        {
            public override Domain Codomain { get; protected init; } = Domain.Boolean;
        }

        partial record Andf
        {
            public override Domain Codomain { get; protected init; } = Domain.Boolean;
        }

        partial record Orf
        {
            public override Domain Codomain { get; protected init; } = Domain.Boolean;
        }

        partial record Xorf
        {
            public override Domain Codomain { get; protected init; } = Domain.Boolean;
        }

        partial record Impliesf
        {
            public override Domain Codomain { get; protected init; } = Domain.Boolean;
        }

        partial record Equalsf
        {
            public override Domain Codomain { get; protected init; } = Domain.Boolean;
        }

        partial record Greaterf
        {
            public override Domain Codomain { get; protected init; } = Domain.Boolean;
        }

        partial record GreaterOrEqualf
        {
            public override Domain Codomain { get; protected init; } = Domain.Boolean;
        }

        partial record Lessf
        {
            public override Domain Codomain { get; protected init; } = Domain.Boolean;
        }

        partial record LessOrEqualf
        {
            public override Domain Codomain { get; protected init; } = Domain.Boolean;
        }

        partial record Set
        {
            partial record FiniteSet
            {
                public override Domain Codomain { get; protected init; } = Domain.Any;
            }

            partial record Interval
            {
                public override Domain Codomain { get; protected init; } = Domain.Any;
            }

            partial record ConditionalSet
            {
                public override Domain Codomain { get; protected init; } = Domain.Any;
            }

            partial record SpecialSet
            {
                public override Domain Codomain { get; protected init; } = Domain.Any;
            }

            partial record Unionf
            {
                public override Domain Codomain { get; protected init; } = Domain.Any;
            }

            partial record Intersectionf
            {
                public override Domain Codomain { get; protected init; } = Domain.Any;
            }

            partial record SetMinusf
            {
                public override Domain Codomain { get; protected init; } = Domain.Any;
            }

            partial record Inf
            {
                public override Domain Codomain { get; protected init; } = Domain.Boolean;
            }
        }
    }
}
