using AngouriMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using static AngouriMath.MathS.UnsafeAndInternal;

namespace Utils
{
    // Currenly works only for `Entity` and its derived classes
    public static class ExportsGenerator
    {
        private static string Pattern(string name)
            => File.ReadAllText($"./Utils/{name}.txt");

        public static string ToExportName(string normalName)
        {
            var sb = new StringBuilder(normalName);
            foreach (var c in normalName)
            {
                if (char.IsUpper(c))
                    sb.Append("_");
                sb.Append(char.ToLower(c));
            }
            return sb.ToString();
        }

        public static IEnumerable<(string name, string exportName, int parCount)> DetectNativeExports(Type type)
        {
            var methods = type.GetMethods()
                .Where(
                method => method.CustomAttributes.OfType<NativeExportAttribute>().Any()
                );
            foreach (var method in methods)
            {
                var name = type.Name + method.Name;
                var exportName = ToExportName(name);
                var pars = method.GetParameters().Count();
                yield return (name, exportName, pars);
            }
        }

        public static void SaveExportingCode(IEnumerable<(string name, string exportName, int parCount)> methods)
        {
            var sg = new SourceGenerator(Pattern("CommonTemplate"), "%bat%", "%usings%", "%namespace%", "%classheader%", "%content%");
            var sb = new StringBuilder();

            foreach (var method in methods)
            {
                var methodSg = new SourceGenerator(
                    Pattern("ExportedFunctions"),
                    "%exportedname%", "%name%", "%params%", "%paramswithouttype%",
                    "%localname%", "%paramswithe%");
                var exportedName = method.exportName;
                var name = method.name;
                var pars = string.Join("", Enumerable.Range(0, method.parCount).Select(c => $"EntityRef arg{c}, "));
                pars += "ref EntityRef res";
                // var parsWi
                // 
            }
        }

        public static void Do()
        {
            
        }
    }
}
