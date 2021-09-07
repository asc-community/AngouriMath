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
