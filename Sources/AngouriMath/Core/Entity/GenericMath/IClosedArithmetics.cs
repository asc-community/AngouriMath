using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    IMultiplyOperators<T, T, T>,
    
    where T : IClosedArithmetics<T>
{
        
}