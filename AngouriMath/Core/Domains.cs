using AngouriMath.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using static AngouriMath.Entity;

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
            => expr is Variable(var name) ? Parse(name) : throw new ParseException($"Unrecognized domain {expr.Stringize()}");

        public static Domain Parse(string name)
            => FromStringToDomain.TryGetValue(name, out var dom) ? dom :
                throw new ParseException($"Unrecognized domain {name}");
    }
}
