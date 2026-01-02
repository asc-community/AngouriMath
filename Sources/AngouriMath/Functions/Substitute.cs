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
        #region Simple

        partial record Matrix
        {
            /// <inheritdoc/>
            public override Entity Substitute(Entity x, Entity value)
                => x == this ? value : Elementwise(c => c.Substitute(x, value));
        }

        partial record Sumf
        {
            /// <inheritdoc/>
            public override Entity Substitute(Entity x, Entity value)
                => this == x ? value : New(Augend.Substitute(x, value), Addend.Substitute(x, value));
        }

        partial record Minusf
        {
            /// <inheritdoc/>
            public override Entity Substitute(Entity x, Entity value)
                => this == x ? value : New(Subtrahend.Substitute(x, value), Minuend.Substitute(x, value));
        }

        partial record Mulf
        {
            /// <inheritdoc/>
            public override Entity Substitute(Entity x, Entity value)
                => this == x ? value : New(Multiplier.Substitute(x, value), Multiplicand.Substitute(x, value));
        }

        partial record Divf
        {
            /// <inheritdoc/>
            public override Entity Substitute(Entity x, Entity value)
                => this == x ? value : New(Dividend.Substitute(x, value), Divisor.Substitute(x, value));
        }

        partial record Sinf
        {
            /// <inheritdoc/>
            public override Entity Substitute(Entity x, Entity value)
                => this == x ? value : New(Argument.Substitute(x, value));
        }

        partial record Cosf
        {
            /// <inheritdoc/>
            public override Entity Substitute(Entity x, Entity value)
                => this == x ? value : New(Argument.Substitute(x, value));
        }

        partial record Secantf
        {
            /// <inheritdoc/>
            public override Entity Substitute(Entity x, Entity value)
                => this == x ? value : New(Argument.Substitute(x, value));
        }

        partial record Cosecantf
        {
            /// <inheritdoc/>
            public override Entity Substitute(Entity x, Entity value)
                => this == x ? value : New(Argument.Substitute(x, value));
        }

        partial record Tanf
        {
            /// <inheritdoc/>
            public override Entity Substitute(Entity x, Entity value)
                => this == x ? value : New(Argument.Substitute(x, value));
        }

        partial record Cotanf
        {
            /// <inheritdoc/>
            public override Entity Substitute(Entity x, Entity value)
                => this == x ? value : New(Argument.Substitute(x, value));
        }

        partial record Logf
        {
            /// <inheritdoc/>
            public override Entity Substitute(Entity x, Entity value)
                => this == x ? value : New(Base.Substitute(x, value), Antilogarithm.Substitute(x, value));
        }

        partial record Powf
        {
            /// <inheritdoc/>
            public override Entity Substitute(Entity x, Entity value)
                => this == x ? value : New(Base.Substitute(x, value), Exponent.Substitute(x, value));
        }

        partial record Arcsinf
        {
            /// <inheritdoc/>
            public override Entity Substitute(Entity x, Entity value)
                => this == x ? value : New(Argument.Substitute(x, value));
        }

        partial record Arccosf
        {
            /// <inheritdoc/>
            public override Entity Substitute(Entity x, Entity value)
                => this == x ? value : New(Argument.Substitute(x, value));
        }

        partial record Arctanf
        {
            /// <inheritdoc/>
            public override Entity Substitute(Entity x, Entity value)
                => this == x ? value : New(Argument.Substitute(x, value));
        }

        partial record Arccotanf
        {
            /// <inheritdoc/>
            public override Entity Substitute(Entity x, Entity value)
                => this == x ? value : New(Argument.Substitute(x, value));
        }

        partial record Arcsecantf
        {
            /// <inheritdoc/>
            public override Entity Substitute(Entity x, Entity value)
                => this == x ? value : New(Argument.Substitute(x, value));
        }

        partial record Arccosecantf
        {
            /// <inheritdoc/>
            public override Entity Substitute(Entity x, Entity value)
                => this == x ? value : New(Argument.Substitute(x, value));
        }

        partial record Factorialf
        {
            /// <inheritdoc/>
            public override Entity Substitute(Entity x, Entity value)
                => this == x ? value : New(Argument.Substitute(x, value));
        }

        partial record Signumf
        {
            /// <inheritdoc/>
            public override Entity Substitute(Entity x, Entity value)
                => this == x ? value : New(Argument.Substitute(x, value));
        }

        partial record Absf
        {
            /// <inheritdoc/>
            public override Entity Substitute(Entity x, Entity value)
                => this == x ? value : New(Argument.Substitute(x, value));
        }

        partial record Boolean
        {
            /// <inheritdoc/>
            public override Entity Substitute(Entity x, Entity value)
                => this == x ? value : this;
        }

        partial record Notf
        {
            /// <inheritdoc/>
            public override Entity Substitute(Entity x, Entity value)
                => this == x ? value : New(Argument.Substitute(x, value));
        }

        partial record Andf
        {
            /// <inheritdoc/>
            public override Entity Substitute(Entity x, Entity value)
                => this == x ? value : New(Left.Substitute(x, value), Right.Substitute(x, value));
        }

        partial record Orf
        {
            /// <inheritdoc/>
            public override Entity Substitute(Entity x, Entity value)
                => this == x ? value : New(Left.Substitute(x, value), Right.Substitute(x, value));
        }

        partial record Xorf
        {
            /// <inheritdoc/>
            public override Entity Substitute(Entity x, Entity value)
                => this == x ? value : New(Left.Substitute(x, value), Right.Substitute(x, value));
        }

        partial record Impliesf
        {
            /// <inheritdoc/>
            public override Entity Substitute(Entity x, Entity value)
                => this == x ? value : New(Assumption.Substitute(x, value), Conclusion.Substitute(x, value));
        }

        partial record Equalsf
        {
            /// <inheritdoc/>
            public override Entity Substitute(Entity x, Entity value)
                => this == x ? value : New(Left.Substitute(x, value), Right.Substitute(x, value));
        }

        partial record Greaterf
        {
            /// <inheritdoc/>
            public override Entity Substitute(Entity x, Entity value)
                => this == x ? value : New(Left.Substitute(x, value), Right.Substitute(x, value));
        }

        partial record GreaterOrEqualf
        {
            /// <inheritdoc/>
            public override Entity Substitute(Entity x, Entity value)
                => this == x ? value : New(Left.Substitute(x, value), Right.Substitute(x, value));
        }

        partial record Lessf
        {
            /// <inheritdoc/>
            public override Entity Substitute(Entity x, Entity value)
                => this == x ? value : New(Left.Substitute(x, value), Right.Substitute(x, value));
        }

        partial record LessOrEqualf
        {
            /// <inheritdoc/>
            public override Entity Substitute(Entity x, Entity value)
                => this == x ? value : New(Left.Substitute(x, value), Right.Substitute(x, value));
        }

        partial record Set
        {
            partial record FiniteSet
            {
                /// <inheritdoc/>
                public override Entity Substitute(Entity x, Entity value)
                    => x == this ? value : Apply(c => c.Substitute(x, value));
            }

            partial record Interval
            {
                /// <inheritdoc/>
                public override Entity Substitute(Entity x, Entity value)
                    => New(Left.Substitute(x, value), Right.Substitute(x, value));
            }

            partial record SpecialSet
            {
                /// <inheritdoc/>
                public override Entity Substitute(Entity x, Entity value)
                    => x == this ? value : this;
            }

            partial record Unionf
            {
                /// <inheritdoc/>
                public override Entity Substitute(Entity x, Entity value)
                    => x == this ? value : New(Left.Substitute(x, value), Right.Substitute(x, value));
            }

            partial record Intersectionf
            {
                /// <inheritdoc/>
                public override Entity Substitute(Entity x, Entity value)
                    => x == this ? value : New(Left.Substitute(x, value), Right.Substitute(x, value));
            }

            partial record SetMinusf
            {
                /// <inheritdoc/>
                public override Entity Substitute(Entity x, Entity value)
                    => x == this ? value : New(Left.Substitute(x, value), Right.Substitute(x, value));
            }

            partial record Inf
            {
                /// <inheritdoc/>
                public override Entity Substitute(Entity x, Entity value)
                    => x == this ? value : New(Element.Substitute(x, value), SupSet.Substitute(x, value));
            }
        }

        partial record Phif
        {
            /// <inheritdoc/>
            public override Entity Substitute(Entity x, Entity value)
                => this == x ? value : New(Argument.Substitute(x, value));
        }

        partial record Providedf
        {
            /// <inheritdoc/>
            public override Entity Substitute(Entity x, Entity value)
                => this == x ? value : New(Expression.Substitute(x, value), Predicate.Substitute(x, value));
        }

        partial record Piecewise
        {
            /// <inheritdoc/>
            public override Entity Substitute(Entity x, Entity value)
                => x == this ? value : Apply(c => c.New(c.Expression.Substitute(x, value), c.Predicate.Substitute(x, value)));
        }

        #endregion

        partial record Application
        {
            /// <inheritdoc/>
            public override Entity Substitute(Entity x, Entity value)
                => x == this ? value : New(Expression.Substitute(x, value), Arguments.Map(arg => arg.Substitute(x, value)));
        }

        #region Local variable preserved
        partial record Set
        {
            partial record ConditionalSet
            {
                // TODO: it might be optimized
                /// <inheritdoc/>
                public override Entity Substitute(Entity x, Entity value)
                {
                    if (this == x)
                        return value;
                    var replacement = Variable.CreateTemp((x + value + Predicate + Var).Vars);

                    // { x | x > a } -> { temp_1 | temp_1 > a }
                    var tempSubstituted = Predicate.Substitute(Var, replacement);

                    // a = 0 -> { temp_1 | temp_1 > a } -> { temp_1 | temp_1 > 0 }
                    // x = 0 -> { temp_1 | temp_1 > a } -> { temp_1 | temp_1 > a }
                    var subs = tempSubstituted.Substitute(x, value);

                    // { temp_1 | temp_1 > a } -> { x | x > a }
                    var postSubs = subs.Substitute(replacement, Var);

                    return New(Var, postSubs);
                }
            }
        }

        partial record Integralf
        {
            // TODO: it might be optimized
            /// <inheritdoc/>
            public override Entity Substitute(Entity x, Entity value)
            {
                if (this == x)
                    return value;
                var replacement = Variable.CreateTemp((x + value + Expression + Var).Vars);

                // integrate(x ^ 2 + a, x) -> integrate(temp_1 ^ 2 + a, temp_1)
                var tempSubstituted = Expression.Substitute(Var, replacement);

                // a = 0 -> integrate(temp_1 ^ 2 + a, temp_1) -> integrate(temp_1 ^ 2 + 0, temp_1)
                // x = 0 -> integrate(temp_1 ^ 2 + a, temp_1) -> integrate(temp_1 ^ 2 + a, temp_1)
                var subs = tempSubstituted.Substitute(x, value);

                // integrate(temp_1 ^ 2 + a, temp_1) -> integrate(x ^ 2 + a, temp_1)
                var postSubs = subs.Substitute(replacement, Var);

                return New(postSubs, Var);
            }
        }

        partial record Derivativef
        {
            // TODO: it might be optimized
            /// <inheritdoc/>
            public override Entity Substitute(Entity x, Entity value)
            {
                if (this == x)
                    return value;
                var replacement = Variable.CreateTemp((x + value + Expression + Var).Vars);

                // derive(x ^ 2 + a, x) -> derive(temp_1 ^ 2 + a, temp_1)
                var tempSubstituted = Expression.Substitute(Var, replacement);

                // a = 0 -> derive(temp_1 ^ 2 + a, temp_1) -> derive(temp_1 ^ 2 + 0, temp_1)
                // x = 0 -> derive(temp_1 ^ 2 + a, temp_1) -> derive(temp_1 ^ 2 + a, temp_1)
                var subs = tempSubstituted.Substitute(x, value);

                // derive(temp_1 ^ 2 + a, temp_1) -> derive(x ^ 2 + a, temp_1)
                var postSubs = subs.Substitute(replacement, Var);

                return New(postSubs, Var);
            }
        }

        partial record Limitf
        {
            // TODO: it might be optimized
            /// <inheritdoc/>
            public override Entity Substitute(Entity x, Entity value)
            {
                if (this == x)
                    return value;
                var replacement = Variable.CreateTemp((x + value + Expression + Var).Vars);

                // derive(x ^ 2 + a, x) -> derive(temp_1 ^ 2 + a, temp_1)
                var tempSubstituted = Expression.Substitute(Var, replacement);

                // a = 0 -> derive(temp_1 ^ 2 + a, temp_1) -> derive(temp_1 ^ 2 + 0, temp_1)
                // x = 0 -> derive(temp_1 ^ 2 + a, temp_1) -> derive(temp_1 ^ 2 + a, temp_1)
                var subs = tempSubstituted.Substitute(x, value);

                // derive(temp_1 ^ 2 + a, temp_1) -> derive(x ^ 2 + a, temp_1)
                var postSubs = subs.Substitute(replacement, Var);

                var dst = Destination.Substitute(x, value);

                return New(postSubs, Var, dst, ApproachFrom);
            }
        }

        partial record Lambda
        {
            /// <inheritdocs/>
            public override Entity Substitute(Entity x, Entity value)
                => Unit.Flow switch
                { 
                    _ when this == x => value,
                    _ when x == Parameter => this,
                    _ when value.Vars.Contains(Parameter) =>
                        Unit.Flow
                            .Let(out var newVar, Variable.CreateUniqueAlphabetFirst(this + value))
                            .ReplaceWith(new Lambda(newVar, Body.Substitute(Parameter, newVar).Substitute(x, value))),
                    _ => New(Parameter, Body.Substitute(x, value))
                };
        }

        #endregion
    }
}
