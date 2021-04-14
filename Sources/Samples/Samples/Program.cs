using System;
using AngouriMath;
using AngouriMath.Extensions;
using static AngouriMath.MathS;
using static AngouriMath.Entity;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using AngouriMath.Core;
using AngouriMath.Core.Exceptions;
using System.Collections.Generic;
using System.Linq;


foreach (var res in ComputeMaxInt(40).Take(10))
    Console.WriteLine(res);


static Exception? Except(Action action)
{
    try
    {
        action();
    }
    catch (Exception e)
    {
        return e;
    }
    return null;
}


static IEnumerable<int> ComputeMaxInt(int quota)
{
    static int SubAlgo(int curr)
    {
        for (int i = 0; i < 15; i++)
        {
            curr++;
            QuotaCounter.QuotaLeft.Value.DecreaseAndCheck();
        }
        return curr;
    }
    using var _ = QuotaCounter.QuotaLeft.Set(QuotaLeft.CreateFinite(quota));
    var curr = 0;
    while (true)
    {
        if (Except(() => curr = SubAlgo(curr)) is OutOfQuotaException)
        {
            yield return curr;
            QuotaCounter.QuotaLeft.Value.Reset();
        }
    }
}

