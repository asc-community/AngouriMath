//
// Copyright (c) 2019-2021 Angouri.
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
            /// <inheritdoc/>
            public override string Stringize() => Name;
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }
    }
}