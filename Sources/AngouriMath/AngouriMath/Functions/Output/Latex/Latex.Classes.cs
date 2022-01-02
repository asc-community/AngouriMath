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
            /// List of constants LaTeX will correctly display
            /// Yet to be extended
            /// Case does matter, not all letters have both displays in LaTeX
            /// </summary>
            [ConstantField]
            private static readonly HashSet<string> LatexisableConstants = new HashSet<string>
            {
                "alpha", "beta", "gamma", "delta", "epsilon", "varepsilon", "zeta", "eta", "theta", "vartheta",
                "iota", "kappa", "varkappa", "lambda", "mu", "nu", "xi", "omicron", "pi", "varpi", "rho",
                "varrho", "sigma", "varsigma", "tau", "upsilon", "phi", "varphi", "chi", "psi", "omega",

                "Gamma", "Delta", "Theta", "Lambda", "Xi", "Pi", "Sigma", "Upsilon", "Phi", "Psi", "Omega",
            };

            private static string LatexiseIfCan(string symbol)
                => LatexisableConstants.Contains(symbol) ? $@"\{symbol}" : symbol;

            /// <summary>
            /// Returns latexised const if it is possible to latexise it,
            /// or its original name otherwise
            /// </summary>
            public override string Latexise() =>
                SplitIndex() is var (prefix, index)
                ?
                $"{LatexiseIfCan(prefix)}_{{{LatexiseIfCan(index)}}}"
                :
                LatexiseIfCan(Name);
        }
    }
}
