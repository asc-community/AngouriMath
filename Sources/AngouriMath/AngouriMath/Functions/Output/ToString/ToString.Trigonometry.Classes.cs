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
            public override string Stringize() => $"sin({Argument.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Cosf
        {
            /// <inheritdoc/>
            public override string Stringize() => $"cos({Argument.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Secantf
        {
            /// <inheritdoc/>
            public override string Stringize() => $"sec({Argument.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Cosecantf
        {
            /// <inheritdoc/>
            public override string Stringize() => $"csc({Argument.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Arcsecantf
        {
            /// <inheritdoc/>
            public override string Stringize() => $"arcsec({Argument.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Arccosecantf
        {
            /// <inheritdoc/>
            public override string Stringize() => $"arccsc({Argument.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Tanf
        {
            /// <inheritdoc/>
            public override string Stringize() => $"tan({Argument.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Cotanf
        {
            /// <inheritdoc/>
            public override string Stringize() => $"cotan({Argument.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Arcsinf
        {
            /// <inheritdoc/>
            public override string Stringize() => $"arcsin({Argument.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Arccosf
        {
            /// <inheritdoc/>
            public override string Stringize() => $"arccos({Argument.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Arctanf
        {
            /// <inheritdoc/>
            public override string Stringize() => $"arctan({Argument.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Arccotanf
        {
            /// <inheritdoc/>
            public override string Stringize() => $"arccotan({Argument.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }
    }
}
