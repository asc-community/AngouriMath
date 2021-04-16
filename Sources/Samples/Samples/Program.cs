using System;
using AngouriMath.Core;
using AngouriMath.Core.Exceptions;
using System.Collections.Generic;
using System.Linq;


Console.WriteLine(Simplify(expr: 0, quota: 40));


static TOut? TryExecuteConstrained<TIn, TOut>(TIn input, int quota, Func<TIn, TOut> alg)
{
    using var _ = QuotaCounter.QuotaLeft.Set(QuotaLeft.CreateFinite(quota));
    try
    {
        return alg(input);
    }
    catch (OutOfQuotaInterruption)
    {
        return default;
    }
}

static void AssignIfCan<TIn>(ref TIn toWhat, TIn? value)
{
    if (value is { } valid)
        toWhat = valid;
}

static int Simplify(int expr, int quota)
{
    var subQuota = 5;
    while (quota > 0)
    {
        AssignIfCan(ref expr, TryExecuteConstrained(expr, subQuota, SubSim1));
        AssignIfCan(ref expr, TryExecuteConstrained(expr, subQuota, SubSim2));
        quota -= subQuota * 2; 
        subQuota += 2;
    }
    return expr;

    static int SubSim1(int curr)
    {
        for (int i = 0; i < 100; i++)
        {
            QuotaCounter.QuotaLeft.Value.DecreaseAndCheck();
            curr++;
        }
        return curr;
    }

    static int SubSim2(int curr)
    {
        for (int i = 0; i < 70; i++)
        {
            QuotaCounter.QuotaLeft.Value.DecreaseAndCheck();
            curr += 2;
        }
        return curr;
    }
}

