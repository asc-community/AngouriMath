/*
 * Copyright (c) 2019-2020 Angourisoft
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
namespace AngouriMath
{
    partial record Entity
    {
        #region Simple
        partial record Variable
        {
            /// <inheritdoc/>
            public override Entity Substitute(Entity x, Entity value)
                => this == x ? value : this;
        }

        partial record Tensor
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


        #endregion

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

        #endregion
    }
}
