//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

namespace AngouriMath
{
    partial record Entity
    {
        public partial record Variable
        {
            /// <summary>
            /// List of letters LaTeX will correctly display
            /// Yet to be extended
            /// Case does matter, for example \Alpha does not exist because the latin A is used instead
            /// </summary>
            [ConstantField] private static readonly HashSet<string> LatexisableConstants =
            [
                "alpha", "beta", "gamma", "delta", "epsilon", "varepsilon", "zeta", "eta", "theta", "vartheta",
                "iota", "kappa", "varkappa", "lambda", "mu", "nu", "xi", "omicron", "pi", "varpi", "rho",
                "varrho", "sigma", "varsigma", "tau", "upsilon", "phi", "varphi", "chi", "psi", "omega",

                "Gamma", "Delta", "Theta", "Lambda", "Xi", "Pi", "Sigma", "Upsilon", "Phi", "Psi", "Omega",
            ];
            internal bool IsLatexUprightFormatted => SplitIndex() is var (prefix, _) ? IsNameLatexUprightFormatted(prefix) : IsNameLatexUprightFormatted(Name);
            internal static bool IsNameLatexUprightFormatted(string varName) =>
                // NOTE: Mathematical constants like "pi" and "e" are rendered upright following ISO 80000-2.
                // This applies everywhere: as main variables or as subscripts.
                ConstantList.ContainsKey(varName) ||
                // NOTE: Multi-character identifiers are rendered upright.
                // This distinguishes multi-character variable names (e.g., "velocity", "temp", "mass")
                // from products of single-letter variables (e.g., v·e·l·o·c·i·t·y).
                // Single-letter variables remain italic as per standard mathematical typography.
                varName.Length > 1 && !LatexisableConstants.Contains(varName);
            /// <summary>
            /// Returns latexised const if it is possible to latexise it,
            /// or its original name otherwise
            /// </summary>
            public override string Latexise()
            {
                static string LatexisePart(string symbol)
                {
                    var inner = LatexisableConstants.Contains(symbol) ? $@"\{symbol}" : symbol;
                    return IsNameLatexUprightFormatted(symbol) ? $@"\mathrm{{{inner}}}" : inner;
                }
                // For variables with subscripts (e.g., "pi_2", "x_e", "e_pi")
                // Both the main part and subscript are processed through LatexisePart,
                // which handles upright formatting for "pi" and "e" consistently
                return SplitIndex() is var (prefix, index)
                    ? $"{LatexisePart(prefix)}_{{{LatexisePart(index)}}}"
                    : LatexisePart(Name);
            }
        }
    }
}
