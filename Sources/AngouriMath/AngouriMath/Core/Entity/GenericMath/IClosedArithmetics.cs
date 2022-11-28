//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Numerics;

namespace AngouriMath.Core;

/// <summary>
/// Represents operators closed in respect to the
/// set of integer, rational, real, complex numbers
/// and matrices.
/// </summary>
public interface IClosedArithmetics<T> :
    IUnaryNegationOperators<T, T>,
    IUnaryPlusOperators<T, T>,

    IAdditionOperators<T, T, T>,
    ISubtractionOperators<T, T, T>,
    IMultiplyOperators<T, T, T>
    
    where T : IClosedArithmetics<T>
{
        
}