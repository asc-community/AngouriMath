//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Numerics;

namespace AngouriMath.Core;

/// <summary>
/// Entities which have
/// both multiplicative and additive
/// identities.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IHasNeutralValues<T> :
    IMultiplicativeIdentity<T, T>,
    IAdditiveIdentity<T, T>
    where T : IHasNeutralValues<T>
{
    
}
