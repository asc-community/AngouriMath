//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.IO;
using System.Reflection;
using System.Runtime;

namespace Utils
{
    class Program
    {
        public static string GetPathIntoSources()
        {
            var path = Directory.GetCurrentDirectory();
            while (Path.GetFileName(path) != "Sources" && path is not "")
                path = Path.GetDirectoryName(path);
            return path ?? "";
        }

        static void Main(string[] args)
        {
            if (args.Length != 1)
                throw new InvalidOperationException("Specify class' name whose method to call");
            
            var className = args[0];

            var typeToCall = Type.GetType("Utils." + className);

            if (typeToCall is null)
                throw new EntryPointNotFoundException($"Class {className} not found");

            MethodInfo? methodDo;

            try
            {
                methodDo = typeToCall.GetMethod("Do");
            }
            catch (AmbiguousMatchException amb)
            {
                throw new AmbiguousImplementationException("There should be one Do", amb);
            }

            if (methodDo is null)
                throw new AmbiguousImplementationException("There should be one Do");

            methodDo.Invoke(null, new object[] { });
        }
    }
}
