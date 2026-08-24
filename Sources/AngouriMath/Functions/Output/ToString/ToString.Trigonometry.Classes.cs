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
            private protected override string StringizeNode() => $"sin({Argument.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Cosf
        {
            /// <inheritdoc/>
            private protected override string StringizeNode() => $"cos({Argument.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Secantf
        {
            /// <inheritdoc/>
            private protected override string StringizeNode() => $"sec({Argument.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Cosecantf
        {
            /// <inheritdoc/>
            private protected override string StringizeNode() => $"csc({Argument.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Arcsecantf
        {
            /// <inheritdoc/>
            private protected override string StringizeNode() => $"arcsec({Argument.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Arccosecantf
        {
            /// <inheritdoc/>
            private protected override string StringizeNode() => $"arccsc({Argument.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Tanf
        {
            /// <inheritdoc/>
            private protected override string StringizeNode() => $"tan({Argument.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Cotanf
        {
            /// <inheritdoc/>
            private protected override string StringizeNode() => $"cotan({Argument.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Arcsinf
        {
            /// <inheritdoc/>
            private protected override string StringizeNode() => $"arcsin({Argument.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Arccosf
        {
            /// <inheritdoc/>
            private protected override string StringizeNode() => $"arccos({Argument.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Arctanf
        {
            /// <inheritdoc/>
            private protected override string StringizeNode() => $"arctan({Argument.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Arccotanf
        {
            /// <inheritdoc/>
            private protected override string StringizeNode() => $"arccotan({Argument.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }
    }
}
