/*
 * Copyright (c) 2019-2020 Angourisoft
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using AngouriMath.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Set;

namespace AngouriMath.Core
{
    /// <summary>
    /// Specify the domain used within a record 
    /// </summary>
    public enum Domain
    {
        Boolean,
        Integer,
        Rational,
        Real,
        Complex,
        Any
    }

    internal static class DomainsFunctional
    {
        public static readonly Type[] Types = new[]
        {
            typeof(Entity.Boolean),
            typeof(Entity.Number.Integer),
            typeof(Entity.Number.Rational),
            typeof(Entity.Number.Real),
            typeof(Entity.Number.Complex)
        };
        public static bool FitsDomainOrNonNumeric(Entity entity, Domain domain)
            => domain == Domain.Any || 
            entity is Entity.Variable ||
            entity.DirectChildren.Count > 0 ||
            Types[(int)domain].IsAssignableFrom(entity.GetType());

        // that should only be used in parser

        private readonly static (Domain domain, string name)[] DomainStringPairs = new[] 
        { 
            (Domain.Boolean, "BB"),
            (Domain.Integer, "ZZ"),
            (Domain.Rational, "QQ"),
            (Domain.Real, "RR"),
            (Domain.Complex, "CC"),
            (Domain.Any, "AA")
        };

        private readonly static Dictionary<string, Domain> FromStringToDomain = 
            DomainStringPairs.ToDictionary(x => x.name, x => x.domain);

        private readonly static Dictionary<Domain, string> FromDomainToString =
            DomainStringPairs.ToDictionary(x => x.domain, x => x.name);

        public static string DomainToString(Domain domain)
            => FromDomainToString.TryGetValue(domain, out var res) ? res : throw new AngouriBugException("Unrecognized domain");

        public static Domain Parse(Entity expr)
            => expr is SpecialSet ss ? ss.SetType : (expr is Variable(var name) ? Parse(name) : throw new UnrecognizedDomainException(expr.Stringize()));

        public static Domain Parse(string name)
            => FromStringToDomain.TryGetValue(name, out var dom) ? dom :
                throw new UnrecognizedDomainException(name);
    }
}
