/*
 * Copyright (c) 2019-2020 Angourisoft
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath
{
    partial record Entity
    {
        public partial record Sinf
        {
            /// <inheritdoc/>
            public override string Latexise() =>
                @"\sin\left(" + Argument.Latexise() + @"\right)";
        }

        public partial record Cosf
        {
            /// <inheritdoc/>
            public override string Latexise() =>
                @"\cos\left(" + Argument.Latexise() + @"\right)";
        }

        public partial record Secantf
        {
            /// <inheritdoc/>
            public override string Latexise() =>
                @"\sec\left(" + Argument.Latexise() + @"\right)";
        }

        public partial record Cosecantf
        {
            /// <inheritdoc/>
            public override string Latexise() =>
                @"\csc\left(" + Argument.Latexise() + @"\right)";
        }

        public partial record Arcsecantf
        {
            /// <inheritdoc/>
            public override string Latexise() =>
                @"\arcsec\left(" + Argument.Latexise() + @"\right)";
        }

        public partial record Arccosecantf
        {
            /// <inheritdoc/>
            public override string Latexise() =>
                @"\arccsc\left(" + Argument.Latexise() + @"\right)";
        }

        public partial record Tanf
        {
            /// <inheritdoc/>
            public override string Latexise() =>
                @"\tan\left(" + Argument.Latexise() + @"\right)";
        }

        public partial record Cotanf
        {
            /// <inheritdoc/>
            public override string Latexise() =>
                @"\cot\left(" + Argument.Latexise() + @"\right)";
        }

        public partial record Arcsinf
        {
            /// <inheritdoc/>
            public override string Latexise() =>
                @"\arcsin\left(" + Argument.Latexise() + @"\right)";
        }

        public partial record Arccosf
        {
            /// <inheritdoc/>
            public override string Latexise() =>
                @"\arccos\left(" + Argument.Latexise() + @"\right)";
        }

        public partial record Arctanf
        {
            /// <inheritdoc/>
            public override string Latexise() =>
                @"\arctan\left(" + Argument.Latexise() + @"\right)";
        }

        public partial record Arccotanf
        {
            /// <inheritdoc/>
            public override string Latexise() =>
                @"\arccot\left(" + Argument.Latexise() + @"\right)";
        }
    }
}
