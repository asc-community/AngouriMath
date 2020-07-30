
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

namespace AngouriMath.Core.FromString
{
    internal static class SyntaxInfo
    {
        internal static readonly string goodCharsForNumbers = "1234567890i.";
        internal static readonly string goodCharsForVars = "qwertyuiopasdfghjklzxcvbnm";
        internal static readonly string goodCharsForOperators = "+-*/^,";
        internal static int GetFuncArg(string name) => goodStringsForFunctions[name.Substring(0, name.Length - 1)];
        internal static readonly Dictionary<string, int> goodStringsForFunctions = new Dictionary<string, int> {
            { "sin", 1 },
            { "cos", 1 },
            { "log", 2 },
            { "sqrt", 1 },
            { "sqr", 1 },
            { "ln", 1 },
            { "tan", 1 },
            { "cotan", 1 },
            { "sec", 1 },
            { "cosec", 1 },
            { "arcsin", 1 },
            { "arccos", 1 },
            { "arctan", 1 },
            { "arccotan", 1 },
            { "arcsec", 1 },
            { "arccosec", 1 },
            { "gamma", 1 },
        };
        internal static readonly Dictionary<char, string> operatorNames = new Dictionary<char, string>
        {
            { '+', "sumf" },
            { '-', "minusf" },
            { '*', "mulf" },
            { '/', "divf" },
            { '^', "powf" }
        };
        internal static readonly Dictionary<char, int> operatorPriorities = new Dictionary<char, int>
        {
            { '+', Const.PRIOR_SUM },
            { '-', Const.PRIOR_MINUS },
            { '*', Const.PRIOR_MUL },
            { '/', Const.PRIOR_DIV },
            { '^', Const.PRIOR_POW }
        };
    }
}
