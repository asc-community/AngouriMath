//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

namespace AngouriMath
{
    internal static class InnerEvalZeroedSinCosConditions
    {
        internal static bool IsSinDefinitelyZero(Entity angle)
            => 
               // ignore too big entities
               angle.Complexity < 100
               
               // is it of the form n * pi?
               && MathS.UnsafeAndInternal.DivideByEntityStrict(angle, MathS.pi)?.Evaled is Integer;
        
        internal static bool IsCosDefinitelyZero(Entity angle)
            => 
                // ignore too big entities
                angle.Complexity < 100
               
                // is it of the form n / m * pi?
                && MathS.UnsafeAndInternal.DivideByEntityStrict(angle, MathS.pi)?.Evaled is Rational angleRat
                
                // if n / m is integer + 1/2
                
                // if it's divided by 1/2
                && angleRat % TrigonometricAngleExpansion.OneHalf == 0
                
                // but is not integer
                && angleRat is not Integer;
    }
    
    partial record Entity
    {
        public partial record Sinf
        {
            private protected override Entity IntrinsicCondition => Boolean.True;
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
                        { Evaled: Complex n } arg when TrigonometryTableValues.PullSin(n, out var res) => res,
                        _ => null
                    },
                    (@this, a) => ((Sinf)@this).New(a),
                    true);
        }

        public partial record Cosf
        {
            private protected override Entity IntrinsicCondition => Boolean.True;
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
            private protected override Entity IntrinsicCondition => !MathS.Cos(Argument).Equalizes(0); // Sec(x) = 1/cos(x) is undefined when cos(x) = 0
            /// <inheritdoc/>
            protected override Entity InnerEval() =>
                ExpandOnOneArgument(Argument.InnerSimplified,
                    a => a switch
                    {
                        var arg when InnerEvalZeroedSinCosConditions.IsCosDefinitelyZero(arg) => MathS.NaN,
                        var n => (n.Evaled as Complex)?.Pipe(Secant)
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
            private protected override Entity IntrinsicCondition => !MathS.Sin(Argument).Equalizes(0); // Csc(x) = 1/sin(x) is undefined when sin(x) = 0
            /// <inheritdoc/>
            protected override Entity InnerEval() =>
                ExpandOnOneArgument(Argument.InnerSimplified,
                    a => a switch
                    {
                        var arg when InnerEvalZeroedSinCosConditions.IsSinDefinitelyZero(arg) => MathS.NaN,
                        var n => (n.Evaled as Complex)?.Pipe(Cosecant)
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
            private protected override Entity IntrinsicCondition => MathS.Abs(Argument) >= 1; // Arcsec is defined for |x| >= 1
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
            private protected override Entity IntrinsicCondition => MathS.Abs(Argument) >= 1; // Arccsc is defined for |x| >= 1
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
            private protected override Entity IntrinsicCondition => !MathS.Cos(Argument).Equalizes(0); // Tan(x) = sin(x)/cos(x) is undefined when cos(x) = 0
            /// <inheritdoc/>
            protected override Entity InnerEval() =>
                ExpandOnOneArgument(Argument.InnerSimplified,
                    a => a switch
                    {
                        var arg when InnerEvalZeroedSinCosConditions.IsCosDefinitelyZero(arg) => MathS.NaN,
                        var n => (n.Evaled as Complex)?.Pipe(Number.Tan)
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
            private protected override Entity IntrinsicCondition => !MathS.Sin(Argument).Equalizes(0); // Cotan(x) = cos(x)/sin(x) is undefined when sin(x) = 0
            /// <inheritdoc/>
            protected override Entity InnerEval() =>
                ExpandOnOneArgument(Argument.InnerSimplified,
                    a => a switch
                    {
                        var arg when InnerEvalZeroedSinCosConditions.IsSinDefinitelyZero(arg) => MathS.NaN,
                        var n => (n.Evaled as Complex)?.Pipe(Number.Cotan)
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
            // Arcsin is defined for |x| <= 1 if we restrict to reals, but everywhere in complex
            private protected override Entity IntrinsicCondition => Boolean.True;
            
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
            // Arccos is defined for |x| <= 1 if we restrict to reals, but everywhere in complex
            private protected override Entity IntrinsicCondition => Boolean.True;
            
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
            private protected override Entity IntrinsicCondition => Boolean.True;
            
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
            private protected override Entity IntrinsicCondition => Boolean.True;
            
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
