//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Collections.Generic;
using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Common
{
    /// <summary>
    /// Every member here takes both an <see cref="IEnumerable{T}"/> and a <c>params Entity[]</c>.
    /// While <c>Entity</c> also had an implicit conversion from <c>List&lt;Entity&gt;</c>, a list
    /// argument matched the params overload in its expanded form as well, neither candidate was
    /// better than the other, and none of these calls compiled -- CS0121. The conversion is gone;
    /// this file failing to compile is what says it has not come back.
    /// </summary>
    [Trait("Area", "Common")]
    public sealed class ListArgumentOverloadTest
    {
        static List<Entity> Equations => new() { "x - 1", "y - 2" };

        [Fact]
        public void EquationSystemTakesAList()
        {
            var solutions = new EquationSystem(Equations).Solve("x", "y");
            Assert.NotNull(solutions);
            Assert.Equal(1, solutions.RowCount);
            Assert.Equal(2, solutions.ColumnCount);
        }

        [Fact]
        public void MathSEquationsTakesAList()
        {
            var solutions = MathS.Equations(Equations).Solve("x", "y");
            Assert.NotNull(solutions);
            Assert.Equal(1, solutions.RowCount);
        }

        [Fact]
        public void FiniteSetTakesAList()
        {
            var set = new Entity.Set.FiniteSet(new List<Entity> { 1, 2, 3 });
            Assert.Equal(3, set.Count);
        }

        /// <summary>The array and variadic forms were never ambiguous, and must stay that way.</summary>
        [Fact]
        public void ArrayAndVariadicFormsStillBind()
        {
            Entity[] equations = { "x - 1", "y - 2" };
            Assert.NotNull(new EquationSystem(equations).Solve("x", "y"));
            Assert.NotNull(new EquationSystem("x - 1", "y - 2").Solve("x", "y"));
            Assert.Equal(3, new Entity.Set.FiniteSet(1, 2, 3).Count);
        }

        /// <summary>
        /// An array in an <c>Entity</c> position is no longer a set. Both conversions are
        /// gone: an array carries an order and repeats, a set has neither, so the conversion
        /// silently discarded part of what it was handed — and it is the one that made the
        /// overloads above ambiguous in the first place. Building a set says so instead.
        /// </summary>
        /// <remarks>
        /// This compiling is what says the conversion has not come back; the assertion is
        /// only that the explicit forms mean what they used to.
        /// </remarks>
        [Fact]
        public void AnArrayIsBuiltIntoASetExplicitly()
        {
            var elements = new Entity[] { 1, 2, 3 };
            Assert.Equal(new Entity.Set.FiniteSet(1, 2, 3), new Entity.Set.FiniteSet(elements));
            Assert.Equal(new Entity.Set.FiniteSet(1, 2, 3), elements.ToSet());
        }
    }
}
