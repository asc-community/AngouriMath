//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.IO;
using System.Linq;

namespace Utils
{
    public static class CopyCovReport
    {
        private static void Log(string msg)
        {
            Console.WriteLine("CopyCovReport: " + msg);
        }

        public static void Do()
        {
            Log("We are in " + Path.GetFullPath("./"));
            var sources = Path.GetFullPath(Program.GetPathIntoSources());
            Log("Sources are " + sources);

            var root = Path.Join(sources, "Tests", "UnitTests");
            var testResults = Path.Join(root, "TestResults");
            Log("Path to take results from: " + Path.GetFullPath(testResults));


            var dirList = Directory.GetDirectories(testResults);
            if (!dirList.Any())
                throw new Exception("No directory found");
            if (dirList.Count() > 1)
                throw new Exception($"More than one directory found: {string.Join(", ", dirList)}");
            var dir = dirList.First();
            Log("Found dir: " + Path.GetFullPath(dir));


            var fileList = Directory.GetFiles(dir);
            if (!fileList.Any())
                throw new Exception("No report found");
            if (fileList.Count() > 1)
                throw new Exception($"More than one report found: {string.Join(", ", fileList)}");
            var file = fileList.First();
            Log("Found file: " + Path.GetFullPath(file));

            var fileName = Path.GetFileName(file);
            var destination = Path.Join(root, fileName);
            if (File.Exists(destination))
            {
                File.Delete(destination);
                Log("The file was deleted: " + Path.GetFullPath(destination));
            }


            var source = file;
            File.Copy(source, destination);
            Log($"Copied from {Path.GetFullPath(source)} to {Path.GetFullPath(destination)}");
        }
    }
}
