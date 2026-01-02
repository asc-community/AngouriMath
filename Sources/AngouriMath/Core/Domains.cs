//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

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
