/* 
 * Copyright (c) 2019-2021 Angouri.
 * AngouriMath is licensed under MIT. 
 * Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
 * Website: https://am.angouri.org.
 */
using AngouriMath.Functions;
using static AngouriMath.Entity.Number;

namespace AngouriMath
{
    partial record Entity
    {
        public partial record Sinf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() =>
                ExpandOnOneArgument(Argument.Evaled,
                    a => a switch
                    {
                        Complex n => Number.Sin(n),
                        _ => null
                    },
                    (@this, a) => ((Sinf)@this).New(a)
                    );

            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                ExpandOnOneArgument(Argument.InnerSimplified,
                    a => a switch
                    {
                        { Evaled: Complex n } when TrigonometryTableValues.PullSin(n, out var res) => res,
                        _ => null
                    },
                    (@this, a) => ((Sinf)@this).New(a),
                    true);
        }

        public partial record Cosf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() =>
                ExpandOnOneArgument(Argument.Evaled,
                    a => a switch
                    {
                        Complex n => Number.Cos(n),
                        _ => null
                    },
                    (@this, a) => ((Cosf)@this).New(a)
                    );

            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                ExpandOnOneArgument(Argument.InnerSimplified,
                    a => a switch
                    {
                        { Evaled: Complex n } when TrigonometryTableValues.PullCos(n, out var res) => res,
                        _ => null
                    },
                    (@this, a) => ((Cosf)@this).New(a),
                    true);
        }

        public partial record Secantf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() =>
                ExpandOnOneArgument(Argument.Evaled,
                    a => a switch
                    {
                        Complex n => Number.Secant(n),
                        _ => null
                    },
                    (@this, a) => ((Secantf)@this).New(a)
                    );

            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                ExpandOnOneArgument(Argument.InnerSimplified,
                    a => a switch
                    {
                        { Evaled: Complex n } when TrigonometryTableValues.PullCos(n, out var res) => (1 / res).InnerSimplified,
                        _ => null
                    },
                    (@this, a) => ((Secantf)@this).New(a),
                    true);
        }

        public partial record Cosecantf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() =>
                ExpandOnOneArgument(Argument.Evaled,
                    a => a switch
                    {
                        Complex n => Number.Cosecant(n),
                        _ => null
                    },
                    (@this, a) => ((Cosecantf)@this).New(a)
                    );

            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                ExpandOnOneArgument(Argument.InnerSimplified,
                    a => a switch
                    {
                        { Evaled: Complex n } when TrigonometryTableValues.PullSin(n, out var res) => (1 / res).InnerSimplified,
                        _ => null
                    },
                    (@this, a) => ((Cosecantf)@this).New(a)
                    , true);
        }

        public partial record Arcsecantf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() =>
                ExpandOnOneArgument(Argument.Evaled,
                    a => a switch
                    {
                        Complex n => Number.Arcsecant(n),
                        _ => null
                    },
                    (@this, a) => ((Arcsecantf)@this).New(a)
                    );

            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                ExpandOnOneArgument(Argument.InnerSimplified,
                    a => a switch
                    {
                        _ => null
                    },
                    (@this, a) => ((Arcsecantf)@this).New(a),
                    true);
        }

        public partial record Arccosecantf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() =>
                ExpandOnOneArgument(Argument.Evaled,
                    a => a switch
                    {
                        Complex n => Number.Arccosecant(n),
                        _ => null
                    },
                    (@this, a) => ((Arccosecantf)@this).New(a)
                    );

            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                ExpandOnOneArgument(Argument.InnerSimplified,
                    a => a switch
                    {
                        _ => null
                    },
                    (@this, a) => ((Arccosecantf)@this).New(a),
                    true);
        }

        public partial record Tanf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() =>
                ExpandOnOneArgument(Argument.Evaled,
                    a => a switch
                    {
                        Complex n => Number.Tan(n),
                        _ => null
                    },
                    (@this, a) => ((Tanf)@this).New(a)
                    );

            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                ExpandOnOneArgument(Argument.InnerSimplified,
                    a => a switch
                    {
                        { Evaled: Complex n } when TrigonometryTableValues.PullTan(n, out var res) => res,
                        _ => null
                    },
                    (@this, a) => ((Tanf)@this).New(a)
                    , true);
        }
        public partial record Cotanf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() =>
                ExpandOnOneArgument(Argument.Evaled,
                    a => a switch
                    {
                        Complex n => Number.Cotan(n),
                        _ => null
                    },
                    (@this, a) => ((Cotanf)@this).New(a)
                    );

            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                ExpandOnOneArgument(Argument.InnerSimplified,
                    a => a switch
                    {
                        { Evaled: Complex n } when TrigonometryTableValues.PullTan(n, out var res) => (1 / res).InnerSimplified,
                        _ => null
                    },
                    (@this, a) => ((Cotanf)@this).New(a)
                    , true);
        }

        public partial record Arcsinf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() =>
                ExpandOnOneArgument(Argument.Evaled,
                a => a switch
                {
                    Complex n => Number.Arcsin(n),
                    _ => null
                },
                (@this, a) => ((Arcsinf)@this).New(a)
                );
            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                ExpandOnOneArgument(Argument.InnerSimplified,
                a => a switch
                {
                    _ => null
                },
                (@this, a) => ((Arcsinf)@this).New(a),
                true);
        }
        public partial record Arccosf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() =>
                ExpandOnOneArgument(Argument.Evaled,
                a => a switch
                {
                    Complex n => Number.Arccos(n),
                    _ => null
                },
                (@this, a) => ((Arccosf)@this).New(a)
                );
            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                ExpandOnOneArgument(Argument.InnerSimplified,
                a => a switch
                {
                    _ => null
                },
                (@this, a) => ((Arccosf)@this).New(a),
                true);
        }
        public partial record Arctanf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() =>
                ExpandOnOneArgument(Argument.Evaled,
                a => a switch
                {
                    Complex n => Number.Arctan(n),
                    _ => null
                },
                (@this, a) => ((Arctanf)@this).New(a)
                );
            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                ExpandOnOneArgument(Argument.InnerSimplified,
                a => a switch
                {
                    _ => null
                },
                (@this, a) => ((Arctanf)@this).New(a)
                , true);
        }
        public partial record Arccotanf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() =>
                ExpandOnOneArgument(Argument.Evaled,
                a => a switch
                {
                    Complex n => Number.Arccotan(n),
                    _ => null
                },
                (@this, a) => ((Arccotanf)@this).New(a)
                );
            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                ExpandOnOneArgument(Argument.InnerSimplified,
                a => a switch
                {
                    _ => null
                },
                (@this, a) => ((Arccotanf)@this).New(a)
                , true);
        }
    }
}
