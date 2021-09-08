using System;
using System.Diagnostics.CodeAnalysis;

namespace AngouriMath;

partial record Entity :
    IParseable<Entity>,

    IEqualityOperators<Entity, Entity>,

    IHasNeutralValues<Entity>,

    IClosedArithmetics<Entity>,
    IDivisionOperators<Entity, Entity, Entity>
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
