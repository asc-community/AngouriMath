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

        public static string GenerateTupleToInterval()
        {
            var types = new[] { "int", "double", "float", "string" };
            var contentTemplate = Pattern("TupleToIntervalTemplate");
            var gen = new SourceGenerator(contentTemplate, "%type1%", "%type2%");
            var sb = new StringBuilder();
            foreach (var type1 in types)
                foreach (var type2 in types)
                    sb.Append(gen.Generate(type1, type2));
            return sb.ToString();
        }


        public const int LONGEST_TUPLE_LENGTH = 9;
        public static string GenerateTupleEquationSystem()
        {
            var contentTemplate = Pattern("EquationSystemFunctionPattern");
            var gen = new SourceGenerator(contentTemplate, "%i%", "%tupleargs%", "%vars%", "%argspassed%", "%varspassed%");

            var sb = new StringBuilder();

            for (int i = 2; i <= LONGEST_TUPLE_LENGTH; i++)
            {
                var tupleargs = string.Join(", ", Enumerable.Range(1, i).Select(c => "string eq" + c));
                var vars = string.Join(", ", Enumerable.Range(1, i).Select(c => "string var" + c));
                var argspassed = string.Join(", ", Enumerable.Range(1, i).Select(c => "eqs.eq" + c));
                var varspassed = string.Join(", ", Enumerable.Range(1, i).Select(c => "var" + c));
                sb.Append(gen.Generate(i.ToString(), tupleargs, vars, argspassed, varspassed));
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
