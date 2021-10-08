//
// Copyright (c) 2019-2021 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

namespace AngouriMath;

partial record Entity
{
    /// <summary>
    /// Returns if an expression
    /// is identical to another expression
    /// assuming the difference between
    /// real numbers less than <paramref name="error"/>
    /// to be negligible.
    /// </summary>
    /// <param name="other"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public bool EqualsImprecisely(Entity other, Real error)
        => EqualsImpreciselyInner(other, error);

    private protected virtual bool EqualsImpreciselyInner(Entity other, Real error)
    {
        if (other.GetType() != GetType())
            return false;
        foreach (var (a, b) in (this.DirectChildren, other.DirectChildren).ZipLists<Entity, Entity, IReadOnlyList<Entity>, IReadOnlyList<Entity>>())
            if (!a.EqualsImpreciselyInner(b, error))
                return false;
        return true;
    }
}

partial record Entity
{
partial record Number
{
partial record Complex
{
    private protected override bool EqualsImpreciselyInner(Entity other, Real error)
    {
        if (other is not Complex c)
            return false;
        if (ImaginaryPart.IsZero && c.ImaginaryPart.IsZero)
            return RealsAreEqual(RealPart, c.RealPart, error);
        return RealsAreEqual(RealPart, c.RealPart, error) && RealsAreEqual(ImaginaryPart, c.ImaginaryPart, error);
                  
        static bool RealsAreEqual(Real r1, Real r2, Real error)
            => r1 == r2 || Number.CtxSubtract(r1.EDecimal, r2.EDecimal).Abs().LessThan(error.EDecimal);
    }
}
}
}

partial record Entity
{
partial record Set
{
partial record FiniteSet
{
    private protected override bool EqualsImpreciselyInner(Entity other, Real error)
    {
        if (other is not FiniteSet otherFinite)
            return false;
    }
}
}
}