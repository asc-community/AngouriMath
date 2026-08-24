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
            /// <inheritdoc/>
            private protected override string StringizeNode() => Name;
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Constant
        {
            /// <inheritdoc/>
            /// <remarks>
            /// A record synthesizes <see cref="object.ToString"/> from its members unless the type
            /// declares one, which is why every node here declares one. A constant prints as its
            /// name in both roles, so a bound occurrence reads back as it was written.
            /// </remarks>
            public override string ToString() => Stringize();
        }
    }
}