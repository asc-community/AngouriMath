using System;

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
    }
}
