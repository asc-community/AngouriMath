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
            public override string Latexize() =>
                @"\sin\left(" + Argument.Latexize() + @"\right)";
        }

        public partial record Cosf
        {
            /// <inheritdoc/>
            public override string Latexize() =>
                @"\cos\left(" + Argument.Latexize() + @"\right)";
        }

        public partial record Secantf
        {
            /// <inheritdoc/>
            public override string Latexize() =>
                @"\sec\left(" + Argument.Latexize() + @"\right)";
        }

        public partial record Cosecantf
        {
            /// <inheritdoc/>
            public override string Latexize() =>
                @"\csc\left(" + Argument.Latexize() + @"\right)";
        }

        public partial record Arcsecantf
        {
            /// <inheritdoc/>
            public override string Latexize() =>
                @"\operatorname{arcsec}\left(" + Argument.Latexize() + @"\right)";
        }

        public partial record Arccosecantf
        {
            /// <inheritdoc/>
            public override string Latexize() =>
                @"\operatorname{arccsc}\left(" + Argument.Latexize() + @"\right)";
        }

        public partial record Tanf
        {
            /// <inheritdoc/>
            public override string Latexize() =>
                @"\tan\left(" + Argument.Latexize() + @"\right)";
        }

        public partial record Cotanf
        {
            /// <inheritdoc/>
            public override string Latexize() =>
                @"\cot\left(" + Argument.Latexize() + @"\right)";
        }

        public partial record Arcsinf
        {
            /// <inheritdoc/>
            public override string Latexize() =>
                @"\arcsin\left(" + Argument.Latexize() + @"\right)";
        }

        public partial record Arccosf
        {
            /// <inheritdoc/>
            public override string Latexize() =>
                @"\arccos\left(" + Argument.Latexize() + @"\right)";
        }

        public partial record Arctanf
        {
            /// <inheritdoc/>
            public override string Latexize() =>
                @"\arctan\left(" + Argument.Latexize() + @"\right)";
        }

        public partial record Arccotanf
        {
            /// <inheritdoc/>
            public override string Latexize() =>
                @"\operatorname{arccot}\left(" + Argument.Latexize() + @"\right)";
        }
    }
}
