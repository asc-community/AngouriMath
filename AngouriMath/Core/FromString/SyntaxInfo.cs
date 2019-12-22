using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.Core.FromString
{
    internal static class SyntaxInfo
    {
        internal static readonly string goodCharsForNumbers = "1234567890i.";
        internal static readonly string goodCharsForVars = "qwertyuiopasdfghjklzxcvbnm";
        internal static readonly string goodCharsForOperators = "+-*/^,";
        internal static readonly Dictionary<string, int> goodStringsForFunctions = new Dictionary<string, int> {
            { "sin", 1 },
            { "cos", 1 },
            { "log", 2 },
            { "sqrt", 1 },
            { "sqr", 1 },
            { "ln", 1 },
            { "tan", 1 },
            { "cotan", 1 },
            { "b", 1 },
            { "tb", 1 },
            { "sec", 1 },
            { "cosec", 1 },
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
