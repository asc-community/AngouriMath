
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

using System.Collections.Generic;
using System.Linq;
using System.Text;
using PeterO.Numbers;

namespace AngouriMath
{
    using Core;
    using static Entity.Number;
    public abstract partial record Entity : ILatexiseable
    {
        /// <summary>Returns the expression in LaTeX (for example, a / b -> \frac{a}{b})</summary>
        public abstract string Latexise();
        protected internal string Latexise(bool parenthesesRequired) =>
            parenthesesRequired ? @$"\left({Latexise()}\right)" : Latexise();

        public partial record Variable
        {
            /// <summary>
            /// List of constants LaTeX will correctly display
            /// Yet to be extended
            /// Case does matter, not all letters have both displays in LaTeX
            /// </summary>
            private static readonly HashSet<string> LatexisableConstants = new HashSet<string>
            {
                "alpha", "beta", "gamma", "delta", "epsilon", "varepsilon", "zeta", "eta", "theta", "vartheta",
                "iota", "kappa", "varkappa", "lambda", "mu", "nu", "xi", "omicron", "pi", "varpi", "rho",
                "varrho", "sigma", "varsigma", "tau", "upsilon", "phi", "varphi", "chi", "psi", "omega",

                "Gamma", "Delta", "Theta", "Lambda", "Xi", "Pi", "Sigma", "Upsilon", "Phi", "Psi", "Omega",
            };

            /// <summary>
            /// Returns latexised const if it is possible to latexise it,
            /// or its original name otherwise
            /// </summary>
            public override string Latexise() =>
                SplitIndex() is var (prefix, index)
                ? (LatexisableConstants.Contains(prefix) ? @"\" + prefix : prefix)
                  + "_{" + index + "}"
                : LatexisableConstants.Contains(Name) ? @"\" + Name : Name;
        }

        public partial record Tensor
        {
            public override string Latexise()
            {
                if (IsMatrix)
                {
                    var sb = new StringBuilder();
                    sb.Append(@"\begin{pmatrix}");
                    var lines = new List<string>();
                    for (int x = 0; x < Shape[0]; x++)
                    {
                        var items = new List<string>();

                        for (int y = 0; y < Shape[1]; y++)
                            items.Add(this[x, y].Latexise());

                        var line = string.Join(" & ", items);
                        lines.Add(line);
                    }
                    sb.Append(string.Join(@"\\", lines));
                    sb.Append(@"\end{pmatrix}");
                    return sb.ToString();
                }
                else if (IsVector)
                {
                    var sb = new StringBuilder();
                    sb.Append(@"\begin{bmatrix}");
                    sb.Append(string.Join(" & ", InnerTensor.Iterate().Select(k => k.Value.Latexise())));
                    sb.Append(@"\end{bmatrix}");
                    return sb.ToString();
                }
                else
                {
                    return this.ToString();
                }
            }
        }
    }
}
