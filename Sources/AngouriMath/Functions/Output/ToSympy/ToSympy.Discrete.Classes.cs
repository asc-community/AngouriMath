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
            internal override string ToSymPy()
                => this ? "True" : "False";
        }

        partial record Notf
        {
            internal override string ToSymPy()
                => $"not {Argument.ToSymPy(Argument.Priority < Priority)}";
        }

        partial record Andf
        {
            internal override string ToSymPy()
                => $"{Left.ToSymPy(Left.Priority < Priority)} and {Right.ToSymPy(Right.Priority < Priority)}";
        }

        partial record Orf
        {
            internal override string ToSymPy()
                => $"{Left.ToSymPy(Left.Priority < Priority)} or {Right.ToSymPy(Right.Priority < Priority)}";
        }

        partial record Xorf
        {
            internal override string ToSymPy()
                => $"{Left.ToSymPy(Left.Priority < Priority)} ^ {Right.ToSymPy(Right.Priority < Priority)}";
        }

        partial record Impliesf
        {
            internal override string ToSymPy()
                => $"sympy.Implies({Assumption.ToSymPy()}, {Conclusion.ToSymPy()})";
        }

        partial record Equalsf
        {
            internal override string ToSymPy()
                => $"{Left.ToSymPy(Left.Priority < Priority)} == {Right.ToSymPy(Right.Priority < Priority)}";
        }

        partial record Greaterf
        {
            internal override string ToSymPy()
                => $"{Left.ToSymPy(Left.Priority < Priority)} > {Right.ToSymPy(Right.Priority < Priority)}";
        }

        partial record GreaterOrEqualf
        {
            internal override string ToSymPy()
                => $"{Left.ToSymPy(Left.Priority < Priority)} >= {Right.ToSymPy(Right.Priority < Priority)}";
        }

        partial record Lessf
        {
            internal override string ToSymPy()
                => $"{Left.ToSymPy(Left.Priority < Priority)} < {Right.ToSymPy(Right.Priority < Priority)}";
        }

        partial record LessOrEqualf
        {
            internal override string ToSymPy()
                => $"{Left.ToSymPy(Left.Priority < Priority)} <= {Right.ToSymPy(Right.Priority < Priority)}";
        }
    }
}
