//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Numerics;

namespace AngouriMath.Core;

/// <summary>
/// Represents comparable items
/// of <see cref="IClosedArithmetics{T}" />
/// </summary>
public interface IScalarClosedArithmetics<T> :
    IClosedArithmetics<T>,
    IComparisonOperators<T, T, bool>

    where T : IScalarClosedArithmetics<T>
{
}
