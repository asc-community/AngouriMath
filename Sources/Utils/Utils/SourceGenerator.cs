//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Text;

namespace Utils
{
    // Yes, I must be inventing wheel... but why adding yet another dependency
    // for such a simple action?
    public sealed class SourceGenerator
    {
        private readonly string[] toReplace;
        private readonly string template;
        public SourceGenerator(string template, params string[] toReplace)
        {
            this.toReplace = toReplace;
            this.template = template;
        }

        public string Generate(params string[] replacements)
        { 
            if (replacements.Length != toReplace.Length)
                throw new ArgumentException();
            var sb = new StringBuilder(template);
            for (int i = 0; i < toReplace.Length; i++)
                sb.Replace(toReplace[i], replacements[i]);
            return sb.ToString();
        }
    }
}
