//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Exceptions;

namespace AngouriMath
{
    partial record Entity
    {
        public partial record Derivativef
        {
            /// <inheritdoc/>
            public override string Stringize()
            {
                if (Iterations == 1)
                    return $"derivative({Expression.Stringize()}, {Var.Stringize()})";
                else
                    return $"derivative({Expression.Stringize()}, {Var.Stringize()}, {Iterations})";
            }
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Integralf
        {
            /// <inheritdoc/>
            public override string Stringize()
            {
                if (Iterations == 1)
                    return $"integral({Expression.Stringize()}, {Var.Stringize()})";
                else
                    return $"integral({Expression.Stringize()}, {Var.Stringize()}, {Iterations})";
            }
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Limitf
        {
            /// <inheritdoc/>
            public override string Stringize() =>

                ApproachFrom switch
                {
                    ApproachFrom.Left => "limitleft",
                    ApproachFrom.BothSides => "limit",
                    ApproachFrom.Right => "limitright",
                    _ => throw new AngouriBugException
                        ($"Unresolved enum {ApproachFrom}")
                } + $"({Expression.Stringize()}, {Var.Stringize()}, {Destination.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }
    }
}