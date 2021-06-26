
/*
 *
 * Great thanks to Andrey Kurdyumov for help! 
 *
 */

using System;

namespace AngouriMath.CPP.Exporting
{
    internal static partial class Exports
    {
        internal static NErrorCode ExceptionEncode<TIn, TOut>(ref TOut destination, TIn input, Func<TIn, TOut> func)
        {
            try
            {
                destination = func(input);
                return NErrorCode.Ok;
            }
            catch (Exception e)
            {
                return NErrorCode.Thrown(e);
            }
        }

        internal static NErrorCode ExceptionEncode<TIn>(TIn input, Action<TIn> func)
        {
            try
            {
                func(input);
                return NErrorCode.Ok;
            }
            catch (Exception e)
            {
                return NErrorCode.Thrown(e);
            }
        }
    }
}
