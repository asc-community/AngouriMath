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
        public partial record Sinf
        {
            internal override string ToSymPy() => "sympy.sin(" + Argument.ToSymPy() + ")";
        }

        public partial record Cosf
        {
            internal override string ToSymPy() => "sympy.cos(" + Argument.ToSymPy() + ")";
        }

        public partial record Secantf
        {
            internal override string ToSymPy() => "sympy.sec(" + Argument.ToSymPy() + ")";
        }

        public partial record Cosecantf
        {
            internal override string ToSymPy() => "sympy.csc(" + Argument.ToSymPy() + ")";
        }

        public partial record Tanf
        {
            internal override string ToSymPy() => "sympy.tan(" + Argument.ToSymPy() + ")";
        }

        public partial record Cotanf
        {
            internal override string ToSymPy() => "sympy.cot(" + Argument.ToSymPy() + ")";
        }

        public partial record Arcsinf
        {
            internal override string ToSymPy() => "sympy.asin(" + Argument.ToSymPy() + ")";
        }

        public partial record Arccosf
        {
            internal override string ToSymPy() => "sympy.acos(" + Argument.ToSymPy() + ")";
        }

        public partial record Arctanf
        {
            internal override string ToSymPy() => "sympy.atan(" + Argument.ToSymPy() + ")";
        }

        public partial record Arccotanf
        {
            internal override string ToSymPy() => "sympy.acot(" + Argument.ToSymPy() + ")";
        }

        public partial record Arcsecantf
        {
            internal override string ToSymPy() => "sympy.asec(" + Argument.ToSymPy() + ")";
        }

        public partial record Arccosecantf
        {
            internal override string ToSymPy() => "sympy.acsc(" + Argument.ToSymPy() + ")";
        }
    }
}
