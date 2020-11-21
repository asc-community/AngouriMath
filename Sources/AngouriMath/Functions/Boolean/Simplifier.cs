/*
 * Copyright (c) 2019-2020 Angourisoft
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using System.Collections.Generic;
using System.Linq;

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
            private struct OrfBranchGetter : IBranchGetter<Orf> { public Entity Left(Orf node) => node.Left; public Entity Right(Orf node) => node.Right; }
            internal static IEnumerable<Entity> LinearChildren(Entity expr)
                => LinearChildren<Orf, OrfBranchGetter>(expr);
        }

        partial record Andf
        {
            private struct AndfBranchGetter : IBranchGetter<Andf> { public Entity Left(Andf node) => node.Left; public Entity Right(Andf node) => node.Right; }
            internal static IEnumerable<Entity> LinearChildren(Entity expr)
                => LinearChildren<Andf, AndfBranchGetter>(expr);
        }

        partial record Set
        {
            partial record Unionf
            {
                private struct UnionfBranchGetter : IBranchGetter<Unionf> { public Entity Left(Unionf node) => node.Left; public Entity Right(Unionf node) => node.Right; }
                internal static IEnumerable<Entity> LinearChildren(Entity expr)
                    => LinearChildren<Unionf, UnionfBranchGetter>(expr);
            }

            partial record Intersectionf
            {
                private struct IntersectionfBranchGetter : IBranchGetter<Intersectionf> { public Entity Left(Intersectionf node) => node.Left; public Entity Right(Intersectionf node) => node.Right; }
                internal static IEnumerable<Entity> LinearChildren(Entity expr)
                    => LinearChildren<Intersectionf, IntersectionfBranchGetter>(expr);
            }
        }
    }
}
