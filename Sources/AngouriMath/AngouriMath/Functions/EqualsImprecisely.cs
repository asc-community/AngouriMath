//
// Copyright (c) 2019-2022 Angouri.
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
    public bool EqualsImprecisely(Entity other, Real error)
        => EqualsImpreciselyInner(other, error);

    
    [ConstantField] private static readonly Real defaultError = 0.01;

    /// <summary>
    /// Returns if an expression
    /// is identical to another expression
    /// assuming the difference between
    /// real numbers less than some small error
    /// to be negligible.
    /// </summary>
    public bool EqualsImprecisely(Entity other)
        => EqualsImpreciselyInner(other, defaultError);

    private protected virtual bool EqualsImpreciselyInner(Entity other, Real error)
    {
        if (other.GetType() != GetType())
            return false;
        if (DirectChildren.Count != other.DirectChildren.Count)
            return false;
        if (DirectChildren.Count == 0)
            return this == other;
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
        if (Count != otherFinite.Count)
            return false;
        return EqualsImpreciselyAlgorithms.InexactEquals(this, otherFinite, (a, b) => a.EqualsImpreciselyInner(b, error));
    }
}
}
}



internal static class EqualsImpreciselyAlgorithms
{
    // LPeter1997's algo
    internal static bool InexactEquals<T>(IReadOnlyCollection<T> aSet, IReadOnlyCollection<T> bSet, System.Func<T, T, bool> inexactEq)
    {
        if (aSet.Count != bSet.Count) return false;

        // The current edges in the match
        var match = new HashSet<(T A, T B)>();
        // Used vertices from aSet
        var aUsed = new HashSet<T>();
        // Used vertices from bSet
        var bUsed = new HashSet<T>();

        void AddEdge(T a, T b)
        {
            aUsed!.Add(a);
            bUsed!.Add(b);
            match!.Add((a, b));
        }

        void RemoveEdge(T a, T b)
        {
            aUsed!.Remove(a);
            bUsed!.Remove(b);
            match!.Remove((a, b));
        }

        List<(T A, T B)> VertexSequenceToList(Stack<T> stk)
        {
            var result = new List<(T A, T B)>();
            var vertices = stk.Reverse().ToList();
            for (var i = 0; i < vertices.Count - 1; ++i) result.Add(i % 2 == 0 ? (vertices[i], vertices[i + 1]) : (vertices[i + 1], vertices[i]));
            return result;
        }

        List<(T A, T B)>? FindAlternatingPathFrom(HashSet<T> touched, Stack<T> path)
        {
            var top = path.Peek();
            if (path.Count % 2 == 0)
            {
                // If we found an alternating path, return it
                if (!bUsed!.Contains(top)) return VertexSequenceToList(path);

                // We need a node from A on a matched edge
                foreach (var a in aUsed!)
                {
                    // Skip unmatched edges or visited vertices
                    if (path.Contains(a) || !match!.Contains((a, top))) continue;
                    // Continue from the found vertex
                    touched.Add(a);
                    path.Push(a);
                    var result = FindAlternatingPathFrom(touched, path);
                    if (result is not null) return result;
                    path.Pop();
                }
            }
            else
            {
                // We need a node from B on an unmatched edge
                foreach (var b in bSet)
                {
                    // Skip matched edges, visited vertices or pairs that don't even make an edge
                    if (path.Contains(b) || match!.Contains((top, b)) || !inexactEq(top, b)) continue;
                    // Continue from the found vertex
                    touched.Add(b);
                    path.Push(b);
                    var result = FindAlternatingPathFrom(touched, path);
                    if (result is not null) return result;
                    path.Pop();
                }
            }
            return null;
        }

        List<(T A, T B)>? FindAlternatingPath()
        {
            var touched = new HashSet<T>();
            foreach (var a in aSet)
            {
                // If 'a' is used or we have already touched it, skip
                if (aUsed.Contains(a) || !touched.Add(a)) continue;
                // Otherwise try to search an alternating path starting from 'a'
                var pathStack = new Stack<T>();
                pathStack.Push(a);
                var path = FindAlternatingPathFrom(touched, pathStack);
                if (path is not null) return path;
            }
            return null;
        }

        // First we do the simplest, stupidest greedy match to help the algorithm
        foreach (var a in aSet)
        {
            foreach (var b in bSet)
            {
                // Don't consider this 'b', if it's already used
                if (bUsed.Contains(b)) continue;

                if (inexactEq(a, b))
                {
                    // 'a' and 'b' can be considered equal, connect them up
                    AddEdge(a, b);
                    // Go to next 'a'
                    break;
                }
            }
        }


        // Now we have some inexact matching in 'match'

        while (match.Count < aSet.Count)
        {
            // While we can find an alternating path - which is a path starting from and ending in an unmatched vertex,
            // containing matching and not matching edges -, we improve on the match
            var alternatingPath = FindAlternatingPath();
            // If there was no alternating path, we are done
            if (alternatingPath is null) break;

            // Otherwise we can improve the match
            for (var i = 0; i < alternatingPath.Count; ++i)
            {
                var edge = alternatingPath[i];
                if (i % 2 == 0) AddEdge(edge.A, edge.B);
                else RemoveEdge(edge.A, edge.B);
            }
        }

        return match.Count == aSet.Count;
    }
}