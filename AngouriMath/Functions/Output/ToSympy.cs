using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using AngouriMath.Core.TreeAnalysis;

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
            { "logf", "log" }
        };

        private static readonly Dictionary<string, string> OperatorTable = new Dictionary<string, string>
        {
            {"sumf", "+"},
            {"minusf", "-"},
            {"divf", "/"},
            {"mulf", "*"},
            {"powf", "**"},
        };
        private static string ToSympyExpr(Entity expr)
        {
            switch (expr.entType)
            {
                case Entity.EntType.FUNCTION:
                    return "sympy." + FuncTable[expr.Name] + "(" + ToSympyExpr(expr.Children[0]) + ")";
                case Entity.EntType.OPERATOR:
                    return "(" + ToSympyExpr(expr.Children[0]) + OperatorTable[expr.Name] + ToSympyExpr(expr.Children[1]) + ")";
                case Entity.EntType.NUMBER:
                    return expr.ToString().Replace("i", "j");
                case Entity.EntType.VARIABLE:
                    return expr.ToString();
                default:
                    throw new MathSException("Unexpected node type");
            }
        }
        internal static string GenerateCode(Entity expr)
        {
            var sb = new StringBuilder();
            var vars = MathS.GetUniqueVariables(expr);
            sb.Append("import sympy\n\n");
            foreach (var f in vars)
                sb.Append(f.ToString() + " = sympy.Symbol('" + f.ToString() + "')\n");
            sb.Append("\n");
            sb.Append("expr = " + ToSympyExpr(expr));
            return sb.ToString();
        }
    }
}
