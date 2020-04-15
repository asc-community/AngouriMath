
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



﻿using System;
using System.Collections.Generic;
using System.Text;
 using AngouriMath.Core.Sys.Interfaces;

namespace AngouriMath
{
    public abstract partial class Entity : ILatexiseable
    {
        /// <summary>
        /// Substitute a variable with an expression
        /// </summary>
        /// <param name="x">
        /// A name of variable to substitute
        /// </param>
        /// <param name="value">
        /// The value we replace variable with
        /// </param>
        /// <returns></returns>
        public Entity Substitute(string x, Entity value)
            => Substitute(MathS.Var(x), value, false);

        /// <summary>
        /// Substitute a variable with an expression
        /// </summary>
        /// <param name="x">
        /// A variable to substitute
        /// </param>
        /// <param name="value">
        /// The value we replace variable with
        /// </param>
        /// <returns></returns>
        public Entity Substitute(VariableEntity x, Entity value)
            => Substitute(x, value, false);
        public Entity Substitute(VariableEntity x, Entity value, bool inPlace)
        {
            Entity res;
            if (inPlace)
                res = this;
            else
                res = DeepCopy();
            if (res == x)
                return value;
            for (int i = 0; i < res.Children.Count; i++)
                res.Children[i] = res.Children[i].Substitute(x, value, true);
            return res;
        }
    }
}
