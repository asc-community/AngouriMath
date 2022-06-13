//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

/*
 *
 * Great thanks to Andrey Kurdyumov for help! 
 *
 */

using System;

namespace AngouriMath.CPP.Exporting
{
    internal static unsafe partial class Exports
    {
        internal static NErrorCode ExceptionEncode<TIn, TOut>(TOut* destination, TIn input, Func<TIn, TOut> func)
            where TIn : unmanaged
            where TOut : unmanaged
        {
            try
            {
                *destination = func(input);
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
