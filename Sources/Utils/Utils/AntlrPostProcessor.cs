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
    public static class AntlrPostProcessorReplacePublicWithInternal
    {
        public const string ANTLR_PATH = "../AngouriMath/Core/Antlr/";

        private static void ProcessFile(string path)
        {
            var finalPath = Path.Combine(ANTLR_PATH, path);

            var textSb = new StringBuilder(File.ReadAllText(finalPath));
            textSb.Replace("public sealed class", "internal class");
            textSb.Replace("public partial class", "internal partial class");
            textSb.Replace("public interface", "internal interface");
            File.WriteAllText(finalPath, textSb.ToString());
        }

        public static void Do()
        {
            ProcessFile("AngouriMathBaseListener.cs");
            ProcessFile("AngouriMathLexer.cs");
            ProcessFile("AngouriMathListener.cs");
            ProcessFile("AngouriMathParser.cs");
        }
    }
}
