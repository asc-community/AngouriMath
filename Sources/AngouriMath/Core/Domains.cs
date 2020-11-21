/*
 * Copyright (c) 2019-2020 Angourisoft
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using System;
using static AngouriMath.Entity.Set;

namespace AngouriMath.Core
{
    /// <summary>
    /// Specify the domain used within a record 
    /// </summary>
    public enum Domain
    {
        /// <summary>
        /// The domain of all boolean values (true, false)
        /// </summary>
        Boolean,

        /// <summary>
        /// The domain of all integer values
        /// </summary>
        Integer,

        /// <summary>
        /// The domain of all rational values
        /// </summary>
        Rational,

        /// <summary>
        /// The domain of all real values
        /// </summary>
        Real,

        /// <summary>
        /// The domain of all complex values
        /// </summary>
        Complex,

        /// <summary>
        /// The domain of all values (might be removed in the future)
        /// </summary>
        Any
    }

    internal static class DomainsFunctional
    {
        public static bool FitsDomainOrNonNumeric(Entity entity, Domain domain)
            => domain == Domain.Any || SpecialSet.Create(domain).MayContain(entity);

    }
}
