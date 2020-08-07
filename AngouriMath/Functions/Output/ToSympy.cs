
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
using System.Text;

namespace AngouriMath.Functions.Output
{
    internal static class ToSympy
    {
        private static readonly Dictionary<string, string> FuncTable = new Dictionary<string, string>
        {
            { "sinf", "sin" },
            { "cosf", "cos" },
            { "tanf", "tan" },
            { "cotanf", "cotan" },
            { "arcsinf", "asin" },
            { "arccosf", "acos" },
            { "arctanf", "atan" },
            { "arccotanf", "acotan" },
            { "logf", "log" },
            { "factorialf", "factorial" },
            { "derivativef", "derivative" }
        };

        private static readonly Dictionary<string, string> OperatorTable = new Dictionary<string, string>
        {
            {"sumf", "+"},
            {"minusf", "-"},
            {"divf", "/"},
            {"mulf", "*"},
            {"powf", "**"},
        };
        private static string ToSympyExpr(Entity expr) => expr switch
        {
            FunctionEntity _ => "sympy." + FuncTable[expr.Name] + "(" + ToSympyExpr(expr.GetChild(0)) + ")",
            OperatorEntity _ => "(" + ToSympyExpr(expr.GetChild(0)) + OperatorTable[expr.Name] + ToSympyExpr(expr.GetChild(1)) + ")",
            NumberEntity _ => expr.ToString().Replace("i", "j"),
            VariableEntity _ => expr.ToString(),
            _ => throw new Core.Exceptions.UnknownEntityException(),
        };

        /// <summary>
        /// Generates sympy-like code
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        internal static string GenerateCode(Entity expr)
        {
            var sb = new StringBuilder();
            var vars = MathS.Utils.GetUniqueVariables(expr);
            sb.Append("import sympy\n\n");
            foreach (var f in vars)
                sb.Append(f.ToString() + " = sympy.Symbol('" + f.ToString() + "')\n");
            sb.Append("\n");
            sb.Append("expr = " + ToSympyExpr(expr));
            return sb.ToString();
        }
    }
}
