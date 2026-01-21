//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using Antlr4.Runtime.Misc;

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
            protected override Entity InnerSimplify(bool isExact) =>
                ExpandOnOneArgument(Argument,
                    a => a switch
                    {
                        Complex n when !isExact => Number.Sin(n),
                        { Evaled: Complex n } arg when isExact && TrigonometryTableValues.PullSin(n, out var res) => res,
                        _ => null
                    },
                    (@this, a) => ((Sinf)@this).New(a), isExact);
        }

        public partial record Cosf
        {
            private protected override Entity IntrinsicCondition => Boolean.True;
            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact) =>
                ExpandOnOneArgument(Argument,
                    a => a switch
                    {
                        Complex n when !isExact => Number.Cos(n),
                        { Evaled: Complex n } when isExact && TrigonometryTableValues.PullCos(n, out var res) => res,
                        _ => null
                    },
                    (@this, a) => ((Cosf)@this).New(a), isExact);
        }

        public partial record Secantf
        {
            private protected override Entity IntrinsicCondition => !MathS.Cos(Argument).EqualTo(0); // Sec(x) = 1/cos(x) is undefined when cos(x) = 0
            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact) =>
                InnerEvalZeroedSinCosConditions.IsCosDefinitelyZero(Argument.InnerSimplified) ? MathS.NaN
                : ExpandOnOneArgument(Argument, a =>
                    a switch
                    {
                        { Evaled: Complex n } =>
                            isExact
                            ? TrigonometryTableValues.PullCos(n, out var res)
                              ? (1 / res).InnerSimplified
                              : null
                            : Secant(n),
                        _ => null
                    },
                    (@this, a) => ((Secantf)@this).New(a), isExact);
        }

        public partial record Cosecantf
        {
            private protected override Entity IntrinsicCondition => !MathS.Sin(Argument).EqualTo(0); // Csc(x) = 1/sin(x) is undefined when sin(x) = 0
            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact) =>
                InnerEvalZeroedSinCosConditions.IsSinDefinitelyZero(Argument.InnerSimplified) ? MathS.NaN
                : ExpandOnOneArgument(Argument,
                    a => a switch
                    {
                        { Evaled: Complex n } =>
                            isExact
                            ? TrigonometryTableValues.PullSin(n, out var res)
                              ? (1 / res).InnerSimplified
                              : null
                            : Cosecant(n),
                        _ => null
                    },
                    (@this, a) => ((Cosecantf)@this).New(a), isExact);
        }

        public partial record Arcsecantf
        {
            private protected override Entity IntrinsicCondition =>
                Codomain < Domain.Complex
                ? MathS.Abs(Argument) >= 1 // Arcsec is defined for |x| >= 1 for reals
                : !Argument.EqualTo(0); // Arcsec is undefined at 0 in complex
            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact) =>
                ExpandOnOneArgument(Argument,
                    a => a switch
                    {
                        Complex n when !isExact => Arcsecant(n),
                        _ => null
                    },
                    (@this, a) => ((Arcsecantf)@this).New(a), isExact);
        }

        public partial record Arccosecantf
        {
            private protected override Entity IntrinsicCondition =>
                Codomain < Domain.Complex
                ? MathS.Abs(Argument) >= 1 // Arccsc is defined for |x| >= 1 for reals
                : !Argument.EqualTo(0); // Arccsc is undefined at 0 in complex
            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact) =>
                ExpandOnOneArgument(Argument,
                    a => a switch
                    {
                        Complex n when !isExact => Number.Arccosecant(n),
                        _ => null
                    },
                    (@this, a) => ((Arccosecantf)@this).New(a), isExact);
        }

        public partial record Tanf
        {
            private protected override Entity IntrinsicCondition =>
                !MathS.Cos(Argument).EqualTo(0); // Tan(x) = sin(x)/cos(x) is undefined when cos(x) = 0

            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact) =>
                InnerEvalZeroedSinCosConditions.IsCosDefinitelyZero(Argument.InnerSimplified) ? MathS.NaN
                : ExpandOnOneArgument(Argument,
                    a => a switch
                    {
                        { Evaled: Complex n } =>
                            isExact
                            ? TrigonometryTableValues.PullTan(n, out var res) ? res : null
                            : Number.Tan(n),
                        _ => null
                    },
                    (@this, a) => ((Tanf)@this).New(a), isExact);
        }
        
        public partial record Cotanf
        {
            private protected override Entity IntrinsicCondition => !MathS.Sin(Argument).EqualTo(0); // Cotan(x) = cos(x)/sin(x) is undefined when sin(x) = 0
            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact) =>
                InnerEvalZeroedSinCosConditions.IsSinDefinitelyZero(Argument.InnerSimplified) ? MathS.NaN
                : ExpandOnOneArgument(Argument,
                    a => a switch
                    {
                        { Evaled: Complex n } =>
                            isExact
                            ? TrigonometryTableValues.PullTan(n, out var res) ? (1 / res).InnerSimplified : null
                            : Number.Cotan(n),
                        _ => null
                    },
                    (@this, a) => ((Cotanf)@this).New(a), isExact);
        }

        public partial record Arcsinf
        {
            // Arcsin is defined for |x| <= 1 if we restrict to reals, but everywhere in complex
            private protected override Entity IntrinsicCondition =>
                Codomain < Domain.Complex ? MathS.Abs(Argument) <= 1 : Boolean.True;

            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact) =>
                ExpandOnOneArgument(Argument,
                a => a switch
                {
                    Complex n when !isExact => Number.Arcsin(n),
                    _ => null
                },
                (@this, a) => ((Arcsinf)@this).New(a), isExact);
        }
        
        public partial record Arccosf
        {
            // Arccos is defined for |x| <= 1 if we restrict to reals, but everywhere in complex
            private protected override Entity IntrinsicCondition =>
                Codomain < Domain.Complex ? MathS.Abs(Argument) <= 1 : Boolean.True;

            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact) =>
                ExpandOnOneArgument(Argument,
                a => a switch
                {
                    Complex n when !isExact => Number.Arccos(n),
                    _ => null
                },
                (@this, a) => ((Arccosf)@this).New(a), isExact);
        }
        
        public partial record Arctanf
        {
            private protected override Entity IntrinsicCondition => Boolean.True;

            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact) =>
                ExpandOnOneArgument(Argument,
                a => a switch
                {
                    Complex n when !isExact => Number.Arctan(n),
                    Real r when r.EDecimal.IsPositiveInfinity() => MathS.pi / 2,
                    Real r when r.EDecimal.IsNegativeInfinity() => -MathS.pi / 2,
                    _ => null
                },
                (@this, a) => ((Arctanf)@this).New(a), isExact);
        }
        
        public partial record Arccotanf
        {
            private protected override Entity IntrinsicCondition => Boolean.True;

            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact) =>
                ExpandOnOneArgument(Argument,
                a => a switch
                {
                    Complex n when !isExact => Number.Arccotan(n),
                    _ => null
                },
                (@this, a) => ((Arccotanf)@this).New(a), isExact);
        }
    }
}
