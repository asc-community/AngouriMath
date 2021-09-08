using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
