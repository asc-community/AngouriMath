
namespace AngouriMath
{
    partial record Entity
    {
        /// <summary>
        /// Coomain of an expression
        /// If its node value is outside of the domain when evaluated,
        /// it turns into a <see cref="MathS.NaN"/>
        /// </summary>
        public abstract Domain Codomain { get; protected init; }

        /// <summary>
        /// Returns this node with the specified codomain, 
        /// keeping all the subnodes in the same domain they were in
        /// </summary>
        public Entity WithCodomain(Domain newDomain)
            => Codomain == newDomain ? this : this with { Codomain = newDomain };
    }
}
