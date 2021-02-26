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
using AngouriMath.Core.Exceptions;

namespace AngouriMath
{
    partial record Entity
    {
        public partial record Derivativef
        {
            internal override string ToSymPy() => $"sympy.diff({Expression.ToSymPy()}, {Var.ToSymPy()}, {Iterations})";
        }

        public partial record Integralf
        {
            // TODO: The 3rd parameter of sympy.integrate is not interpreted as iterations, unlike sympy.diff
            // which allows both sympy.diff(expr, var, iterations) and sympy.diff(expr, var1, var2, var3...)
            internal override string ToSymPy() => $"sympy.integrate({Expression.ToSymPy()}, {Var.ToSymPy()}, {Iterations})";
        }

        public partial record Limitf
        {
            internal override string ToSymPy() =>
                @$"sympy.limit({Expression.ToSymPy()}, {Var.ToSymPy()}, {Destination.ToSymPy()}{ApproachFrom switch
                {
                    ApproachFrom.Left => ", '-'",
                    ApproachFrom.BothSides => "",
                    ApproachFrom.Right => ", '+'",
                    _ => throw new AngouriBugException
                        ($"Unresolved enum {ApproachFrom}")
                }})";
        }
    }
}
