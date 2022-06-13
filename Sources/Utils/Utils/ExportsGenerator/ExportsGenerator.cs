//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
                var pars = string.Join(", ", Enumerable.Range(0, method.parCount).Select(c => $"ObjRef arg{c}"));
                var parsWithoutType = string.Join(", ", Enumerable.Range(0, method.parCount).Select(c => $"arg{c}"));
                string parsWithE = "";
                if (method.parCount > 1)
                    parsWithE = string.Join(", ", Enumerable.Range(0, method.parCount).Select(c => $"e.arg{c}.AsEntity"));
                else
                    parsWithE = "e.AsEntity";
                sb.Append(methodSg.Generate(
                    exportedName, "E" + name, pars, parsWithoutType, parsWithE, $"{type.FullName?.Replace("+", ".")}.{name}"
                    ));

            }
            
            return sg.Generate(
                "generate_exports.bat",
                "using System.Runtime.InteropServices;",
                "AngouriMath.CPP.Exporting",
                "unsafe partial class Exports", 
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

        public static string SaveUsingCode(IEnumerable<(string name, string exportName, int parCount)> methods, string path)
        {
            var sg = new SourceGenerator(Pattern("ExportsGenerator/UsingFile"), "%importfile%", "%content%");
            var sb = new StringBuilder();
            foreach (var method in methods)
            {
                var name = method.name;
                var exportName = method.exportName;
                var pars = string.Join(", ", Enumerable.Range(0, method.parCount).Select(c => $"const Entity& arg{c}"));
                var parsWithoutType = string.Join(", ", Enumerable.Range(0, method.parCount).Select(c => $"GetHandle(arg{c})"));
                var methodSg = new SourceGenerator(
                    Pattern("ExportsGenerator/UsingFunctions"), 
                    "%name%", "%exportname%", "%params%", "%paramswithouttype%");
                sb.Append(methodSg.Generate(name, exportName, pars, parsWithoutType));
            }
            return sg.Generate(path, sb.ToString());
        }

        private static void Export(Type type)
        {
            Console.WriteLine($"Exporting {type.FullName}...");
            var nativeExports = DetectNativeExports(type);
            Console.WriteLine($"{nativeExports.Count()} native methods detected");

            var csCode = SaveExportingCode(nativeExports, type);
            var path = $"Wrappers/AngouriMath.CPP.Exporting/A.Exports.{type.Name}.Functions.cs";
            File.WriteAllText(
                Path.Combine(
                    Program.GetPathIntoSources(),
                    path
                ),
                csCode
            );
            Console.WriteLine($"{csCode.Length}-long C# exporting code was generated and saved to {path}");

            var cppCode = SaveImportingCode(nativeExports, type);
            path = $"Wrappers/AngouriMath.CPP.Importing/A.Imports.{type.Name}.Functions.h";
            File.WriteAllText(
                Path.Combine(
                    Program.GetPathIntoSources(),
                    path
                ),
                cppCode
            );
            Console.WriteLine($"{cppCode.Length}-long C++ importing code was generated and saved to {path}");

            var name = $"A.Usages.{type.Name}.Functions.h";
            var importName = $"A.Imports.{type.Name}.Functions.h";
            path = "Wrappers/AngouriMath.CPP.Importing/" + name;
            var cppAMCode = SaveUsingCode(nativeExports, importName);
            File.WriteAllText(
                Path.Combine(
                    Program.GetPathIntoSources(),
                    path
                ),
                cppAMCode
            );
            Console.WriteLine($"{cppAMCode.Length}-long C++ API code was generated and saved to {path}");

            Console.WriteLine($"Done exporting {type.FullName}\n\n");
        }

        public static void Do()
        {
            Export(typeof(MathS));
            Export(typeof(MathS.Hyperbolic));
        }
    }
}
