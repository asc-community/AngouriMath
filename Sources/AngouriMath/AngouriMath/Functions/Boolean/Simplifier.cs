//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

namespace AngouriMath
{
    partial record Entity
    {
        // ngl; it looks awful

        private protected interface IBranchGetter<T>
        { 
            Entity Left(T node);
            Entity Right(T node);
        }

        private protected static IEnumerable<Entity> LinearChildren<TNode, TGetter>(Entity expr)
            where TNode : Entity where TGetter : struct, IBranchGetter<TNode>
        {
            if (expr is not TNode node)
                return new[] { expr };
            var (left, right) = (default(TGetter).Left(node), default(TGetter).Right(node));
            return LinearChildren<TNode, TGetter>(left).Concat(LinearChildren<TNode, TGetter>(right));
        }

        partial record Orf
        {
            private struct OrfBranchGetter : IBranchGetter<Orf> { public readonly Entity Left(Orf node) => node.Left; public readonly Entity Right(Orf node) => node.Right; }
            internal static IEnumerable<Entity> LinearChildren(Entity expr)
                => LinearChildren<Orf, OrfBranchGetter>(expr);
        }

        partial record Andf
        {
            private struct AndfBranchGetter : IBranchGetter<Andf> { public readonly Entity Left(Andf node) => node.Left; public readonly Entity Right(Andf node) => node.Right; }
            internal static IEnumerable<Entity> LinearChildren(Entity expr)
                => LinearChildren<Andf, AndfBranchGetter>(expr);
        }

        partial record Xorf
        {
            private struct XorfBranchGetter : IBranchGetter<Xorf> { public readonly Entity Left(Xorf node) => node.Left; public readonly Entity Right(Xorf node) => node.Right; }
            internal static IEnumerable<Entity> LinearChildren(Entity expr)
                => LinearChildren<Xorf, XorfBranchGetter>(expr);

        }

        partial record Set
        {
            partial record Unionf
            {
                private struct UnionfBranchGetter : IBranchGetter<Unionf> { public readonly Entity Left(Unionf node) => node.Left; public readonly Entity Right(Unionf node) => node.Right; }
                internal static IEnumerable<Entity> LinearChildren(Entity expr)
                    => LinearChildren<Unionf, UnionfBranchGetter>(expr);
            }

            partial record Intersectionf
            {
                private struct IntersectionfBranchGetter : IBranchGetter<Intersectionf> { public readonly Entity Left(Intersectionf node) => node.Left; public readonly Entity Right(Intersectionf node) => node.Right; }
                internal static IEnumerable<Entity> LinearChildren(Entity expr)
                    => LinearChildren<Intersectionf, IntersectionfBranchGetter>(expr);
            }
        }
    }
}
