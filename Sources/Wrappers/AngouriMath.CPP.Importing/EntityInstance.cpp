#include "EntityInstance.h"
#include "imports.h"

namespace AngouriMath::Internal
{
    const std::vector<AngouriMath::Entity>& EntityInstance::CachedNodes()
    { 
        auto lambda = [](AngouriMath::Internal::EntityRef _this) {
            NativeArray nRes;
            auto handle = _this;
            HandleErrorCode(entity_nodes(handle, &nRes));
            std::vector<Entity> res(nRes.length);
            for (size_t i = 0; i < nRes.length; i++)
                res[i] = CreateByHandle(nRes.refs[i]);
            free_native_array(nRes);
            const std::vector<AngouriMath::Entity> resFinal = res;
            return resFinal;
        };
        return nodes.GetValue(lambda, ref);
    }
}