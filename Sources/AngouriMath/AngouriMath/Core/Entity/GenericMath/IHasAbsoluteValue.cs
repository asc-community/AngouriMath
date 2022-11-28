//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

namespace AngouriMath.Core;


/// <summary>
/// Values which have absolute value
/// </summary>
public interface IHasAbsoluteValue<TSelf, TOut>
    where TSelf : IHasAbsoluteValue<TSelf, TOut>
{
    /// <summary>
    /// The absolute value of the entity
    /// </summary>
    abstract static TOut Abs(TSelf self);
}
