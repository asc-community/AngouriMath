#pragma once

#include "TypeAliases.h"
#include "FieldCache.h"
#include "AngouriMath.h"

namespace AngouriMath::Internal
{
    class EntityInstance
    {
    private:
        FieldCache<const std::vector<AngouriMath::Entity>> nodes;
    public:
        EntityRef ref;
        EntityInstance(EntityRef ref) : ref(ref) { }
        const std::vector<AngouriMath::Entity> CachedNodes();
    };
}