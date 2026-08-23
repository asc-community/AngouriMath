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
        public partial record Sinf
        {
            /// <inheritdoc/>
            private protected override string LatexizeNode() =>
                @"\sin\left(" + Argument.Latexize() + @"\right)";
        }

        public partial record Cosf
        {
            /// <inheritdoc/>
            private protected override string LatexizeNode() =>
                @"\cos\left(" + Argument.Latexize() + @"\right)";
        }

        public partial record Secantf
        {
            /// <inheritdoc/>
            private protected override string LatexizeNode() =>
                @"\sec\left(" + Argument.Latexize() + @"\right)";
        }

        public partial record Cosecantf
        {
            /// <inheritdoc/>
            private protected override string LatexizeNode() =>
                @"\csc\left(" + Argument.Latexize() + @"\right)";
        }

        public partial record Arcsecantf
        {
            /// <inheritdoc/>
            private protected override string LatexizeNode() =>
                @"\operatorname{arcsec}\left(" + Argument.Latexize() + @"\right)";
        }

        public partial record Arccosecantf
        {
            /// <inheritdoc/>
            private protected override string LatexizeNode() =>
                @"\operatorname{arccsc}\left(" + Argument.Latexize() + @"\right)";
        }

        public partial record Tanf
        {
            /// <inheritdoc/>
            private protected override string LatexizeNode() =>
                @"\tan\left(" + Argument.Latexize() + @"\right)";
        }

        public partial record Cotanf
        {
            /// <inheritdoc/>
            private protected override string LatexizeNode() =>
                @"\cot\left(" + Argument.Latexize() + @"\right)";
        }

        public partial record Arcsinf
        {
            /// <inheritdoc/>
            private protected override string LatexizeNode() =>
                @"\arcsin\left(" + Argument.Latexize() + @"\right)";
        }

        public partial record Arccosf
        {
            /// <inheritdoc/>
            private protected override string LatexizeNode() =>
                @"\arccos\left(" + Argument.Latexize() + @"\right)";
        }

        public partial record Arctanf
        {
            /// <inheritdoc/>
            private protected override string LatexizeNode() =>
                @"\arctan\left(" + Argument.Latexize() + @"\right)";
        }

        public partial record Arccotanf
        {
            /// <inheritdoc/>
            private protected override string LatexizeNode() =>
                @"\operatorname{arccot}\left(" + Argument.Latexize() + @"\right)";
        }
    }
}
