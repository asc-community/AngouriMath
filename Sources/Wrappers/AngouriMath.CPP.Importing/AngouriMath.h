#pragma once

#include "TypeAliases.h"
#include "ErrorCode.h"
#include <memory>
#include <string>
#include <ostream>
#include <vector>
#include <complex>
#include <ratio>
#include "FieldCache.h"

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
        FieldCache<std::shared_ptr<Entity>> innerEvaled;
        FieldCache< std::shared_ptr<Entity>> innerSimplified;
    public:
        EntityRef reference;
        EntityInstance(EntityRef reference) : reference(reference) { }
        const std::vector<Entity>& CachedNodes();
        const std::vector<Entity>& CachedVars();
        const std::vector<Entity>& CachedVarsAndConstants();
        const Entity& CachedEvaled();
        const Entity& CachedInnerSimplified();
    };
}

namespace AngouriMath
{
    enum class ApproachFrom : int
    {
        BothSides = 0,
        Left = 1,
        Right = 2
    };

    class Entity
    {
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
        const Entity InnerSimplified() const { return innerEntityInstance.get()->CachedInnerSimplified(); }
        const Entity Evaled() const { return innerEntityInstance.get()->CachedEvaled(); }

        friend Internal::EntityRef GetHandle(const Entity& e);
        friend Entity CreateByHandle(Internal::EntityRef handle);
    private:
        explicit Entity(Internal::EntityRef handle);
        
        std::shared_ptr<Internal::EntityInstance> innerEntityInstance;
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