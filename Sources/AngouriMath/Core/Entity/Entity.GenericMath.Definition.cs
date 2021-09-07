#if DEBUG
#define NET60
#endif

using System;
using System.Diagnostics.CodeAnalysis;

namespace AngouriMath
{
    #if NET60
    partial record Entity :
        IParseable<Entity>,

        IEqualityOperators<Entity, Entity>,

        IUnaryNegationOperators<Entity, Entity>,
        IUnaryPlusOperators<Entity, Entity>,

        IAdditionOperators<Entity, Entity, Entity>,
        ISubtractionOperators<Entity, Entity, Entity>,
        IMultiplyOperators<Entity, Entity, Entity>,
        IDivisionOperators<Entity, Entity, Entity>,

        IAdditiveIdentity<Entity, Entity>,
        IMultiplicativeIdentity<Entity, Entity>
    {
        /// <summary>
        /// Parses the string into expression.
        /// Ignores the provided format provider.
        /// </summary>
        public static Entity Parse(string s, IFormatProvider? _)
            => s;

        /// <summary>
        /// Tries to parse the string into expression.
        /// Ignores the provided format provider.
        /// </summary>
        public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? _, out Entity result)
            => MathS.Parse(s ?? throw new ArgumentNullException()).Is(out result);

        /// <inheritdoc/>
        public static Entity AdditiveIdentity => 0;

        /// <inheritdoc/>
        public static Entity MultiplicativeIdentity => 1;
    }

    #endif
}
