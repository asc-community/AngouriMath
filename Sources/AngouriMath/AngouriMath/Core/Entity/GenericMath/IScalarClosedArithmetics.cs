//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngouriMath.Core;

/// <summary>
/// Represents comparable items
/// of <see cref="IClosedArithmetics{T}" />
/// </summary>
public interface IScalarClosedArithmetics<T> :
    IClosedArithmetics<T>,
    IComparisonOperators<T, T>

    where T : IScalarClosedArithmetics<T>
{
}
