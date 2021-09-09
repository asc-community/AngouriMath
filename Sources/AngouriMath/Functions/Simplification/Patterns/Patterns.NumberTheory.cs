using static AngouriMath.Entity;

namespace AngouriMath.Functions
{
    internal static partial class Patterns
    {
        internal static Entity PhiFunctionRules(Entity x) => x switch
        {
            Phif(Powf(Integer prime, var variable)) when prime.IsPrime => new Powf(prime, variable - 1) * (prime - 1), 
            _ => x
        };
    }
}
