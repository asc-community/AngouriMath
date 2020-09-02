using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public static class TupleExtensionGenerator
    {
        public const int LONGEST_TUPLE_LENGTH = 9;

        public static void Do()
        {
            var sb = new StringBuilder();
            for (int i = 2; i <= LONGEST_TUPLE_LENGTH; i++)
            {
                sb.Append("public static Tensor? SolveSystem(this (")
                .AppendJoin(", ", Enumerable.Range(1, i).Select(c => "string eq" + c))
                .Append(") eqs, ")
                .AppendJoin(", ", Enumerable.Range(1, i).Select(c => "string var" + c))
                .Append(")\n")
                .Append("    => MathS.Equations(")
                .AppendJoin(", ", Enumerable.Range(1, i).Select(c => "eqs.eq" + c))
                .Append(").Solve(")
                .AppendJoin(", ", Enumerable.Range(1, i).Select(c => "var" + c))
                .Append(");\n\n");
                ;
            }
            Console.WriteLine(sb.ToString());
        }
    }
}
