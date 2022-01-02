//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.IO;
using System.Text;

namespace Utils
{
    public static class AdditionalExtensionsTestGenerator
    {
        private static string Pattern(string name)
            => File.ReadAllText($"./Utils/{name}.txt");

        public static string GenerateTupleToInterval()
        {
            var types = new[] { "3", "4.5", "\"6\"" };
            var contentTemplate = Pattern("TupleToIntervalTest");
            var gen = new SourceGenerator(contentTemplate, "%testid%", "%arg1%", "%arg2%");
            var sb = new StringBuilder();
            var id = 0;
            foreach (var type1 in types)
                foreach (var type2 in types)
                {
                    sb.Append(gen.Generate(id.ToString(), type1, type2));
                    id++;
                }
            return sb.ToString();
        }

        public static void Do()
        {
            var commonTemplate = Pattern("CommonTemplate");

            var com = new SourceGenerator(commonTemplate, "%bat%", "%usings%", "%namespace%", "%classheader%", "%content%");

            var fullText = com.Generate("generate_additional_extensions_tests.bat", "using Xunit;\nusing AngouriMath;\nusing AngouriMath.Extensions;", "UnitTest.Extensions", "public class IntervalExtensionTest", GenerateTupleToInterval());

            File.WriteAllText("../Tests/UnitTests/Convenience/TupleToIntervalTest.cs", fullText);
        }
    }
}
