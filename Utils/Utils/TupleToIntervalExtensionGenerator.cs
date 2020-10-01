using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Utils
{
    public static class TupleToIntervalExtensionGenerator
    {
        public static void Do()
        {
            var types = new [] { "int", "uint", "short", "ushort", "byte", "sbyte", "long", "ulong", "string" };
            var commonTemplate = File.ReadAllText("./Utils/CommonTemplate.txt");
            var contentTemplate = File.ReadAllText("./Utils/TupleToIntervalTemplate.txt");
            var gen = new SourceGenerator(contentTemplate, "%type1%", "%type2%");
            var com = new SourceGenerator(commonTemplate, "%usings%", "%namespace%", "%classheader%", "%content%");
            var sb = new StringBuilder();
            foreach (var type1 in types)
                foreach (var type2 in types)
                    sb.Append(gen.Generate(type1, type2));

            var fullText = com.Generate("using AngouriMath;\nusing static AngouriMath.Entity.Set;", "AngouriMath.Extensions", "public static class AngouriMathExtensions", sb.ToString());

            File.WriteAllText("TupleToIntervalExtensions.cs", fullText);
        }
    }
}
