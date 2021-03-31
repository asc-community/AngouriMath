#pragma once

#include "TypeAliases.h"
#include "ErrorCode.h"
#include <memory>
#include <string>
#include <ostream>
#include <vector>
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
    public:
        EntityRef reference;
        EntityInstance(EntityRef reference) : reference(reference) { }
        const std::vector<Entity>& CachedNodes();
        const std::vector<Entity>& CachedVars();
        const std::vector<Entity>& CachedVarsAndConstants();
    };
}

namespace AngouriMath
{
    class Entity
    {
    public:
        Entity();
        Entity(const std::string& expr);
        Entity(const char* expr);

        std::string ToString() const;
        std::string Latexise() const;
        Entity Differentiate(const Entity& var) const;
        Entity Integrate(const Entity& var) const;
        Entity Limit(const Entity& var, const Entity& dest) const;
        Entity Limit(const Entity& var, const Entity& dest, ApproachFrom from) const;

        const std::vector<Entity>& Nodes() { return innerEntityInstance.get()->CachedNodes(); }
        const std::vector<Entity>& Vars() { return innerEntityInstance.get()->CachedVars(); }
        const std::vector<Entity>& VarsAndConstants() { return innerEntityInstance.get()->CachedVarsAndConstants(); }

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

    enum ApproachFrom : int
    {
        BothSides = 0,
        Left = 1,
        Right = 2
    };
}

namespace std
{
    inline std::string to_string(const AngouriMath::Entity& e)
    {
        return e.ToString();
    }
}