using System;
using System.Collections.Generic;
using System.Text;

namespace MathSharp
{
    public static class Const
    {
        public static readonly int PRIOR_SUM = 2;
        public static readonly int PRIOR_MINUS = 2;
        public static readonly int PRIOR_MUL = 4;
        public static readonly int PRIOR_DIV = 4;
        public static readonly int PRIOR_POW = 6;
        public static readonly int PRIOR_FUNC = 8;
        public static readonly int PRIOR_VAR = 10;
        public static readonly int PRIOR_NUM = 2;
        public static readonly string ARGUMENT_DELIMITER = ",";
    }
}
