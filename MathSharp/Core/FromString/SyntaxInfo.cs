using System;
using System.Collections.Generic;
using System.Text;

namespace MathSharp.Core.FromString
{
    public static class SyntaxInfo
    {
        public static readonly string goodCharsForNumbers = "1234567890i.";
        public static readonly string goodCharsForVars = "qwertyuiopasdfghjklzxcvbnm";
        public static readonly string goodCharsForOperators = "+-*/^,";
        public static readonly Dictionary<string, int> goodStringsForFunctions = new Dictionary<string, int> {
            { "sin", 1 },
            { "cos", 1 },
            { "log", 2 },
            { "sqrt", 1 }
        };
        public static readonly Dictionary<char, string> operatorNames = new Dictionary<char, string>
        {
            { '+', "Sumf" },
            { '-', "Minusf" },
            { '*', "Mulf" },
            { '/', "Divf" },
            { '^', "Powf" }
        };
        public static readonly Dictionary<char, int> operatorPriorities = new Dictionary<char, int>
        {
            { '+', Const.PRIOR_SUM },
            { '-', Const.PRIOR_MINUS },
            { '*', Const.PRIOR_MUL },
            { '/', Const.PRIOR_DIV },
            { '^', Const.PRIOR_POW }
        };
    }
}
