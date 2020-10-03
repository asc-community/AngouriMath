

using AngouriMath;
using System;
using System.Collections.Immutable;

namespace Samples
{
    public record ModuleManifest(string Name, Version Version);


    public record ModuleSet(ImmutableArray<ModuleManifest> Manifests)
    {
        public static ModuleSet Empty { get; } = new ModuleSet(ImmutableArray<ModuleManifest>.Empty);
    }

   
    class Program
    {

        public static ModuleSet ActiveModuleSet { get; private set; } = ModuleSet.Empty;
        static void Main(string[] _)
        {
            Console.WriteLine(ActiveModuleSet.ToString());
        }
    }
}