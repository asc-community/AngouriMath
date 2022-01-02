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
            public override string Latexise() =>
                @"\sin\left(" + Argument.Latexise() + @"\right)";
        }

        public partial record Cosf
        {
            /// <inheritdoc/>
            public override string Latexise() =>
                @"\cos\left(" + Argument.Latexise() + @"\right)";
        }

        public partial record Secantf
        {
            /// <inheritdoc/>
            public override string Latexise() =>
                @"\sec\left(" + Argument.Latexise() + @"\right)";
        }

        public partial record Cosecantf
        {
            /// <inheritdoc/>
            public override string Latexise() =>
                @"\csc\left(" + Argument.Latexise() + @"\right)";
        }

        public partial record Arcsecantf
        {
            /// <inheritdoc/>
            public override string Latexise() =>
                @"\arcsec\left(" + Argument.Latexise() + @"\right)";
        }

        public partial record Arccosecantf
        {
            /// <inheritdoc/>
            public override string Latexise() =>
                @"\arccsc\left(" + Argument.Latexise() + @"\right)";
        }

        public partial record Tanf
        {
            /// <inheritdoc/>
            public override string Latexise() =>
                @"\tan\left(" + Argument.Latexise() + @"\right)";
        }

        public partial record Cotanf
        {
            /// <inheritdoc/>
            public override string Latexise() =>
                @"\cot\left(" + Argument.Latexise() + @"\right)";
        }

        public partial record Arcsinf
        {
            /// <inheritdoc/>
            public override string Latexise() =>
                @"\arcsin\left(" + Argument.Latexise() + @"\right)";
        }

        public partial record Arccosf
        {
            /// <inheritdoc/>
            public override string Latexise() =>
                @"\arccos\left(" + Argument.Latexise() + @"\right)";
        }

        public partial record Arctanf
        {
            /// <inheritdoc/>
            public override string Latexise() =>
                @"\arctan\left(" + Argument.Latexise() + @"\right)";
        }

        public partial record Arccotanf
        {
            /// <inheritdoc/>
            public override string Latexise() =>
                @"\arccot\left(" + Argument.Latexise() + @"\right)";
        }
    }
}
