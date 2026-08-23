//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.IO;
using System.Linq;
using System.Text;

namespace Utils
{
    public static class ExtensionGenerator
    {
        private static string Pattern(string name)
            => File.ReadAllText($"./Utils/{name}.txt");

        // The documented outputs are what the library prints for these very arguments, so a
        // left and a right sample per type are carried alongside the type name: the printed
        // form is not the literal (4.5 comes back as 9/2), and the two ends have to differ or
        // the example degenerates to an empty interval.
        private static readonly (string type, string leftValue, string leftPrinted, string rightValue, string rightPrinted)[] intervalSamples =
        {
            ("int", "2", "2", "5", "5"),
            ("double", "0.5", "1/2", "4.5", "9/2"),
            ("float", "0.5f", "1/2", "4.5f", "9/2"),
            ("string", "\"pi\"", "pi", "\"2 pi\"", "2 * pi"),
        };

        public static string GenerateTupleToInterval()
        {
            var contentTemplate = Pattern("TupleToIntervalTemplate");
            var gen = new SourceGenerator(contentTemplate, "%type1%", "%type2%", "%val1%", "%val2%", "%print1%", "%print2%");
            var sb = new StringBuilder();
            foreach (var left in intervalSamples)
                foreach (var right in intervalSamples)
                    sb.Append(gen.Generate(left.type, right.type, left.leftValue, right.rightValue, left.leftPrinted, right.rightPrinted));
            return sb.ToString();
        }


        public const int LONGEST_TUPLE_LENGTH = 9;

        // e and i are taken -- Euler's number and the imaginary unit -- so the worked example
        // names its unknowns from the letters the parser still reads as variables.
        private static readonly string[] exampleVariables = { "a", "b", "c", "d", "f", "g", "h", "j", "k" };

        public static string GenerateTupleEquationSystem()
        {
            var contentTemplate = Pattern("EquationSystemFunctionPattern");
            var gen = new SourceGenerator(contentTemplate, "%i%", "%tupleargs%", "%vars%", "%argspassed%", "%varspassed%", "%eqlist%", "%varlist%", "%rows%");

            var sb = new StringBuilder();

            for (int i = 2; i <= LONGEST_TUPLE_LENGTH; i++)
            {
                var tupleargs = string.Join(", ", Enumerable.Range(1, i).Select(c => "string eq" + c));
                var vars = string.Join(", ", Enumerable.Range(1, i).Select(c => "string var" + c));
                var argspassed = string.Join(", ", Enumerable.Range(1, i).Select(c => "eqs.eq" + c));
                var varspassed = string.Join(", ", Enumerable.Range(1, i).Select(c => "var" + c));

                // a^2 = 1 pins the first unknown to either sign and the rest follow it, so the
                // system has exactly two solutions whatever its size -- which is what makes the
                // shape of the answer, a row per solution, visible in the printed output.
                var names = exampleVariables.Take(i).ToArray();
                var eqlist = string.Join(", ", names.Select((n, k) => k is 0 ? "\"a^2 - 1\"" : $"\"{n} - {k + 1} * a\""));
                var varlist = string.Join(", ", names.Select(n => $"\"{n}\""));
                var positive = string.Join(", ", Enumerable.Range(1, i));
                var negative = string.Join(", ", Enumerable.Range(1, i).Select(c => -c));
                var rows = $"[[{positive}], [{negative}]]";

                sb.Append(gen.Generate(i.ToString(), tupleargs, vars, argspassed, varspassed, eqlist, varlist, rows));
            }

            return sb.ToString();
        }

        public static void Do()
        {
            var commonTemplate = Pattern("CommonTemplate");
            
            var com = new SourceGenerator(commonTemplate, "%bat%", "%usings%", "%namespace%", "%classheader%", "%content%");
            
            var tupleToInterval = GenerateTupleToInterval();

            var tupleToSystem = GenerateTupleEquationSystem();

            var fullText = com.Generate("generate_additional_extensions.bat", "using static AngouriMath.Entity.Set;\nusing static AngouriMath.Entity;", "AngouriMath.Extensions", "public static partial class AngouriMathExtensions", tupleToSystem + tupleToInterval);

            File.WriteAllText("../AngouriMath/Convenience/AdditionalExtensions.cs", fullText);
        }
    }
}
