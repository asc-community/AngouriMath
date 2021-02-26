/*
 * Copyright (c) 2019-2020 Angourisoft
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

namespace AngouriMath
{
    partial record Entity
    {
        partial record Boolean
        {
            /// <inheritdoc/>
            public override string Latexise() => $@"\operatorname{{{(bool)this}}}";
        }

        partial record Notf
        {
            /// <inheritdoc/>
            public override string Latexise()
                => $@"\neg{{{Argument.Latexise(Argument.Priority < Priority)}}}";
        }

        partial record Andf
        {
            /// <inheritdoc/>
            public override string Latexise()
                => $@"{Left.Latexise(Left.Priority < Priority)} \land {Right.Latexise(Right.Priority < Priority)}";
        }

        partial record Orf
        {
            /// <inheritdoc/>
            public override string Latexise()
                => $@"{Left.Latexise(Left.Priority < Priority)} \lor {Right.Latexise(Right.Priority < Priority)}";
        }

        partial record Xorf
        {
            /// <inheritdoc/>
            public override string Latexise()
                => $@"{Left.Latexise(Left.Priority < Priority)} \oplus {Right.Latexise(Right.Priority < Priority)}";
        }

        partial record Impliesf
        {
            /// <inheritdoc/>
            public override string Latexise()
                => $@"{Assumption.Latexise(Assumption.Priority < Priority)} \implies {Conclusion.Latexise(Conclusion.Priority < Priority)}";
        }

        partial record Equalsf
        {
            /// <inheritdoc/>
            public override string Latexise()
                => $@"{Left.Latexise(Left.Priority < Priority)} = {Right.Latexise(Right.Priority < Priority)}";
        }

        partial record Greaterf
        {
            /// <inheritdoc/>
            public override string Latexise()
                => $@"{Left.Latexise(Left.Priority < Priority)} > {Right.Latexise(Right.Priority < Priority)}";
        }

        partial record GreaterOrEqualf
        {
            /// <inheritdoc/>
            public override string Latexise()
                => $@"{Left.Latexise(Left.Priority < Priority)} \geqslant {Right.Latexise(Right.Priority < Priority)}";
        }

        partial record Lessf
        {
            /// <inheritdoc/>
            public override string Latexise()
                => $@"{Left.Latexise(Left.Priority < Priority)} < {Right.Latexise(Right.Priority < Priority)}";
        }

        partial record LessOrEqualf
        {
            /// <inheritdoc/>
            public override string Latexise()
                => $@"{Left.Latexise(Left.Priority < Priority)} \leqslant {Right.Latexise(Right.Priority < Priority)}";
        }
    }
}
