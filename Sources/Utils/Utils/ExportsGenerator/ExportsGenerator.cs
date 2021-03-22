using AngouriMath;
using System;
using System.Collections.Generic;
using System.IO;
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
            => File.ReadAllText(
                Path.Combine(
                    Program.GetPathIntoSources(),
                    $"Utils/Utils/{name}.txt"
                )
               );

        public static string ToExportName(string normalName)
        {
            var sb = new StringBuilder();
            foreach (var c in normalName)
            {
                if (char.IsUpper(c) && sb.Length != 0)
                    sb.Append("_");
                sb.Append(char.ToLower(c));
            }
            return sb.ToString();
        }

        public static IEnumerable<(string name, string exportName, int parCount)> DetectNativeExports(Type type)
        {
            var methods = type.GetMethods()
                .Where(
                method =>
                method.GetParameters().Any() &&
                method.CustomAttributes.Select(
                    attr => attr.AttributeType.Name == "NativeExportAttribute"
                ).Any()
            );
            foreach (var method in methods)
            {
                var name = method.Name;
                var exportName = ToExportName(type.Name + name);
                var pars = method.GetParameters().Count();
                yield return (name, exportName, pars);
            }
        }

        public static string SaveExportingCode(IEnumerable<(string name, string exportName, int parCount)> methods, Type type)
        {
            var sg = new SourceGenerator(Pattern("CommonTemplate"), "%bat%", "%usings%", "%namespace%", "%classheader%", "%content%");
            var sb = new StringBuilder();

            foreach (var method in methods)
            {
                var methodSg = new SourceGenerator(
                    Pattern("ExportsGenerator/ExportedFunctions"),
                    "%exportedname%", "%name%", "%params%", "%paramswithouttype%",
                    "%paramswithe%", "%localname%");
                var exportedName = method.exportName;
                var name = method.name;
                var pars = string.Join(", ", Enumerable.Range(0, method.parCount).Select(c => $"EntityRef arg{c}"));
                var parsWithoutType = string.Join(", ", Enumerable.Range(0, method.parCount).Select(c => $"arg{c}"));
                string parsWithE = "";
                if (method.parCount > 1)
                    parsWithE = string.Join(", ", Enumerable.Range(0, method.parCount).Select(c => $"e.arg{c}.Entity"));
                else
                    parsWithE = "e.Entity";
                sb.Append(methodSg.Generate(
                    exportedName, "E" + name, pars, parsWithoutType, parsWithE, $"{type.FullName?.Replace("+", ".")}.{name}"
                    ));

            }
            
            return sg.Generate(
                "export_cs_build.bat",
                "using System.Runtime.InteropServices;",
                "AngouriMath.CPP.Exporting",
                "partial class Exports", 
                sb.ToString());
        }

        public static string SaveImportingCode(IEnumerable<(string name, string exportName, int parCount)> methods, Type type)
        {
            var sb = new StringBuilder();

            var sg = new SourceGenerator(Pattern("ExportsGenerator/ImportFile"), "%content%");

            foreach (var method in methods)
            {
                var methodSg = new SourceGenerator(Pattern("ExportsGenerator/ImportedFunctions"), "%exportname%", "%params%");
                var exportName = method.exportName;
                var pars = string.Join("", Enumerable.Range(0, method.parCount).Select(c => "EntityRef, "));
                sb.Append(methodSg.Generate(exportName, pars));
            }

            return sg.Generate(sb.ToString());
        }

        private static void Export(Type type)
        {
            var nativeExports = DetectNativeExports(type);

            var csCode = SaveExportingCode(nativeExports, type);
            File.WriteAllText(
                Path.Combine(
                    Program.GetPathIntoSources(),
                    $"Wrappers/AngouriMath.CPP.Exporting/A.Exports.{type.Name}.Functions.cs"
                ),
                csCode
            );

            var cppCode = SaveImportingCode(nativeExports, type);
            File.WriteAllText(
                Path.Combine(
                    Program.GetPathIntoSources(),
                    $"Wrappers/AngouriMath.CPP.Importing/A.Imports.{type.Name}.Functions.h"
                ),
                cppCode
            );
        }

        public static void Do()
        {
            Export(typeof(MathS));
            Export(typeof(MathS.Hyperbolic));
        }
    }
}
