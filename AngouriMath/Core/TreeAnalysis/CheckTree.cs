
/* Copyright (c) 2019-2020 Angourisoft
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,
 * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software
 * is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */


using System.Linq;
using AngouriMath.Core.FromString;
using AngouriMath.Core.TreeAnalysis;

namespace AngouriMath.Core.TreeAnalysis
{
    public class TreeException : MathSException
    {
        public TreeException(string message) : base(message)
        {
        }
    }

    internal static partial class TreeAnalyzer
    {
        /// <summary>
        /// Checks if a tree contains some bad entites,
        /// for example a variable "sumf".
        /// If something wrong, it throws exception.
        /// Recommended to wrap it into try catch
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        public static void CheckTree(Entity tree)
        {
            tree.Check();
            foreach (var child in tree.ChildrenReadonly)
                CheckTree(child);
        }

        internal static void AssertTree(bool condition, string message)
        {
            if (!condition)
                throw new TreeException(message);
        }

        /// <summary>
        /// Returns whether expr doesn't contain any indefinite numbers
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        internal static bool IsFinite(Entity expr)
        {
            if (expr is NumberEntity { Value:var value })
                return value.IsFinite;
            else
                return expr.ChildrenReadonly.All(IsFinite);
        }
    }
}