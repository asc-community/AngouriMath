using AngouriMath.Core.Exceptions;
using System;
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
        public static Domain Parse(Entity expr)
            => expr switch
            {
                Variable(var name) when name == "ZZ" => Domain.Integer,
                Variable(var name) when name == "QQ" => Domain.Rational,
                Variable(var name) when name == "RR" => Domain.Real,
                Variable(var name) when name == "CC" => Domain.Complex,
                Variable(var name) when name == "BB" => Domain.Boolean,
                _ => throw new ParseException($"Unrecognized domain {expr}")
            };
    }
}
