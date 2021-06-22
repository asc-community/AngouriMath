#pragma once

#include "TypeAliases.h"
#include "ErrorCode.h"
#include "FieldCache.h"

#include <memory>
#include <string>
#include <ostream>
#include <vector>
#include <complex>

namespace AngouriMath
{
    class Entity;
}

namespace AngouriMath::Internal
{
    class EntityInstance
    {
    private:
        FieldCache<std::vector<Entity>> nodes;
        FieldCache<std::vector<Entity>> vars;
        FieldCache<std::vector<Entity>> varsAndConstants;
        FieldCache<std::vector<Entity>> directChildren;
        FieldCache<std::shared_ptr<Entity>> innerEvaled;
        FieldCache<std::shared_ptr<Entity>> innerSimplified;
        FieldCache<std::string> string;
        EntityRef reference;
    public:
        EntityInstance(EntityRef reference) : reference(reference) { }

        const std::vector<Entity>& CachedNodes();
        const std::vector<Entity>& CachedVars();
        const std::vector<Entity>& CachedVarsAndConstants();
        const std::vector<Entity>& CachedDirectChildren();
        EntityRef GetReference() const { return reference; }
        const Entity& CachedEvaled();
        const Entity& CachedInnerSimplified();
        const std::string& CachedString();
    };
}

namespace AngouriMath
{
    enum class ApproachFrom : std::int32_t
    {
        BothSides = 0,
        Left = 1,
        Right = 2
    };

    class Entity
    {
        explicit Entity(Internal::EntityRef handle);

        std::shared_ptr<Internal::EntityInstance> innerEntityInstance;
    public:
        // Constructors
        Entity();
        Entity(const std::string& expr);
        Entity(const char* expr);

        // Methods
        std::string ToString() const;
        std::string Latexise() const;
        Entity Differentiate(const Entity& var) const;
        Entity Integrate(const Entity& var) const;
        Entity Solve(const Entity& var) const;
        Entity SolveEquation(const Entity& var) const;
        Entity Limit(const Entity& var, const Entity& dest) const;
        Entity Limit(const Entity& var, const Entity& dest, ApproachFrom from) const;
        Entity Simplify() const;
        std::vector<Entity> Alternate() const;


        // Casts
        std::int64_t AsInteger() const;
        std::pair<std::int64_t, std::int64_t> AsRational() const;
        double AsReal() const;
        std::complex<double> AsComplex() const;

        // Properties
        const std::vector<Entity>& Nodes() const { return innerEntityInstance.get()->CachedNodes(); }
        const std::vector<Entity>& Vars() const { return innerEntityInstance.get()->CachedVars(); }
        const std::vector<Entity>& VarsAndConsts() const { return innerEntityInstance.get()->CachedVarsAndConstants(); }
        const std::vector<Entity>& DirectChildren() const { return innerEntityInstance.get()->CachedDirectChildren(); }
        const Entity InnerSimplified() const { return innerEntityInstance.get()->CachedInnerSimplified(); }
        const Entity Evaled() const { return innerEntityInstance.get()->CachedEvaled(); }

        friend Internal::EntityRef GetHandle(const Entity& e);
        friend Entity CreateByHandle(Internal::EntityRef handle);
    };

    inline std::ostream& operator<<(std::ostream& out, const AngouriMath::Entity& e)
    {
        out << e.ToString();
        return out;
    }
}

namespace std
{
    inline std::string to_string(const AngouriMath::Entity& e)
    {
        return e.ToString();
    }
}