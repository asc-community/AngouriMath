using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
