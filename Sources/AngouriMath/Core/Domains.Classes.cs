/*
 * Copyright (c) 2019-2020 Angourisoft
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using AngouriMath.Core;

namespace AngouriMath
{
    partial record Entity
    {
        partial record Number
        {
#pragma warning disable SealedOrAbstract // The only exception: those three records are neither abstract nor sealed
            partial record Complex
            {
                /// <inheritdoc/>
                public override Domain Codomain { get; protected init; } = Domain.Complex;
            }

            partial record Rational
            {
                /// <inheritdoc/>
                public override Domain Codomain { get; protected init; } = Domain.Rational;
            }

            partial record Real
            {
                /// <inheritdoc/>
                public override Domain Codomain { get; protected init; } = Domain.Real;
            }
#pragma warning restore SealedOrAbstract

            partial record Integer
            {
                /// <inheritdoc/>
                public override Domain Codomain { get; protected init; } = Domain.Integer;
            }
        }

        partial record Variable
        {
            /// <inheritdoc/>
            public override Domain Codomain { get; protected init; } = Domain.Any;
        }

        partial record Tensor
        {
            /// <inheritdoc/>
            public override Domain Codomain { get; protected init; } = Domain.Any;
        }

        partial record Sumf
        {
            /// <inheritdoc/>
            public override Domain Codomain { get; protected init; } = Domain.Complex;
        }

        partial record Minusf
        {
            /// <inheritdoc/>
            public override Domain Codomain { get; protected init; } = Domain.Complex;
        }

        partial record Mulf
        {
            /// <inheritdoc/>
            public override Domain Codomain { get; protected init; } = Domain.Complex;
        }

        partial record Divf
        {
            /// <inheritdoc/>
            public override Domain Codomain { get; protected init; } = Domain.Complex;
        }

        partial record Sinf
        {
            /// <inheritdoc/>
            public override Domain Codomain { get; protected init; } = Domain.Complex;
        }

        partial record Cosf
        {
            /// <inheritdoc/>
            public override Domain Codomain { get; protected init; } = Domain.Complex;
        }

        partial record Tanf
        {
            /// <inheritdoc/>
            public override Domain Codomain { get; protected init; } = Domain.Complex;
        }

        partial record Cotanf
        {
            /// <inheritdoc/>
            public override Domain Codomain { get; protected init; } = Domain.Complex;
        }

        partial record Logf
        {
            /// <inheritdoc/>
            public override Domain Codomain { get; protected init; } = Domain.Complex;
        }

        partial record Powf
        {
            /// <inheritdoc/>
            public override Domain Codomain { get; protected init; } = Domain.Complex;
        }

        partial record Secantf
        {
            /// <inheritdoc/>
            public override Domain Codomain { get; protected init; } = Domain.Complex;
        }

        partial record Cosecantf
        {
            /// <inheritdoc/>
            public override Domain Codomain { get; protected init; } = Domain.Complex;
        }

        partial record Arcsecantf
        {
            /// <inheritdoc/>
            public override Domain Codomain { get; protected init; } = Domain.Complex;
        }

        partial record Arccosecantf
        {
            /// <inheritdoc/>
            public override Domain Codomain { get; protected init; } = Domain.Complex;
        }

        partial record Arcsinf
        {
            /// <inheritdoc/>
            public override Domain Codomain { get; protected init; } = Domain.Complex;
        }

        partial record Arccosf
        {
            /// <inheritdoc/>
            public override Domain Codomain { get; protected init; } = Domain.Complex;
        }

        partial record Arctanf
        {
            /// <inheritdoc/>
            public override Domain Codomain { get; protected init; } = Domain.Complex;
        }

        partial record Arccotanf
        {
            /// <inheritdoc/>
            public override Domain Codomain { get; protected init; } = Domain.Complex;
        }

        partial record Factorialf
        {
            /// <inheritdoc/>
            public override Domain Codomain { get; protected init; } = Domain.Complex;
        }

        partial record Derivativef
        {
            /// <inheritdoc/>
            public override Domain Codomain { get; protected init; } = Domain.Complex;
        }

        partial record Integralf
        {
            /// <inheritdoc/>
            public override Domain Codomain { get; protected init; } = Domain.Complex;
        }

        partial record Limitf
        {
            /// <inheritdoc/>
            public override Domain Codomain { get; protected init; } = Domain.Complex;
        }

        partial record Signumf
        {
            /// <inheritdoc/>
            public override Domain Codomain { get; protected init; } = Domain.Complex;
        }

        partial record Absf
        {
            /// <inheritdoc/>
            public override Domain Codomain { get; protected init; } = Domain.Real;
        }

        partial record Boolean
        {
            /// <inheritdoc/>
            public override Domain Codomain { get; protected init; } = Domain.Boolean;
        }

        partial record Notf
        {
            /// <inheritdoc/>
            public override Domain Codomain { get; protected init; } = Domain.Boolean;
        }

        partial record Andf
        {
            /// <inheritdoc/>
            public override Domain Codomain { get; protected init; } = Domain.Boolean;
        }

        partial record Orf
        {
            /// <inheritdoc/>
            public override Domain Codomain { get; protected init; } = Domain.Boolean;
        }

        partial record Xorf
        {
            /// <inheritdoc/>
            public override Domain Codomain { get; protected init; } = Domain.Boolean;
        }

        partial record Impliesf
        {
            /// <inheritdoc/>
            public override Domain Codomain { get; protected init; } = Domain.Boolean;
        }

        partial record Equalsf
        {
            /// <inheritdoc/>
            public override Domain Codomain { get; protected init; } = Domain.Boolean;
        }

        partial record Greaterf
        {
            /// <inheritdoc/>
            public override Domain Codomain { get; protected init; } = Domain.Boolean;
        }

        partial record GreaterOrEqualf
        {
            /// <inheritdoc/>
            public override Domain Codomain { get; protected init; } = Domain.Boolean;
        }

        partial record Lessf
        {
            /// <inheritdoc/>
            public override Domain Codomain { get; protected init; } = Domain.Boolean;
        }

        partial record LessOrEqualf
        {
            /// <inheritdoc/>
            public override Domain Codomain { get; protected init; } = Domain.Boolean;
        }

        partial record Set
        {
            partial record FiniteSet
            {
                /// <inheritdoc/>
                public override Domain Codomain { get; protected init; } = Domain.Any;
            }

            partial record Interval
            {
                /// <inheritdoc/>
                public override Domain Codomain { get; protected init; } = Domain.Real;
            }

            partial record ConditionalSet
            {
                /// <inheritdoc/>
                public override Domain Codomain { get; protected init; } = Domain.Any;
            }

            partial record SpecialSet
            {
                /// <inheritdoc/>
                public override Domain Codomain { get; protected init; } = Domain.Any;
            }

            partial record Unionf
            {
                /// <inheritdoc/>
                public override Domain Codomain { get; protected init; } = Domain.Any;
            }

            partial record Intersectionf
            {
                /// <inheritdoc/>
                public override Domain Codomain { get; protected init; } = Domain.Any;
            }

            partial record SetMinusf
            {
                /// <inheritdoc/>
                public override Domain Codomain { get; protected init; } = Domain.Any;
            }

            partial record Inf
            {
                /// <inheritdoc/>
                public override Domain Codomain { get; protected init; } = Domain.Boolean;
            }
        }

        partial record Phif
        {
            /// <inheritdoc/>
            public override Domain Codomain { get; protected init; } = Domain.Integer;
        }

        partial record DefinedWhen
        {
            /// <inheritdoc/>
            public override Domain Codomain { get; protected init; } = Domain.Any;
        }
    }
}
