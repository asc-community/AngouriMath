#include "EntityInstance.h"

namespace AngouriMath::Internal
{
    const std::vector<AngouriMath::Entity> EntityInstance::CachedNodes()
    { 
        return nodes.GetValue([](auto& _this) {  
            NativeArray nRes;
            auto handle = _this.ref;
            HandleErrorCode(entity_nodes(handle, &nRes));
            std::vector<Entity> res(nRes.length);
            for (size_t i = 0; i < nRes.length; i++)
                res[i] = Entity(nRes.refs[i]);
            free_native_array(nRes);
            return res;
        }, ref);
    }
}