/*
 * Copyright (c) 2019-2021 Angouri.
 * AngouriMath is licensed under MIT.
 * Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
 * Website: https://am.angouri.org.
 */

#include "AngouriMath.h"
#include "imports.h"
#include <vector>

namespace AngouriMath
{
    struct HandleDeleter
    {
        void operator()(const Internal::EntityInstance* inner)
        {
            if (inner != nullptr)
            {
                (void)free_entity(inner->reference);
                delete inner;
            }
        }
    };

    Entity::Entity(Internal::EntityRef handle)
        : innerEntityInstance(new Internal::EntityInstance(handle), HandleDeleter())
    {
        
    }

    Internal::EntityRef ParseString(const char* expr)
    {
        Internal::EntityRef result;
        HandleErrorCode(maths_from_string(expr, &result));
        return result;
    }

    Internal::EntityRef ParseString(const char* expr, ErrorCode& e)
    {
        Internal::EntityRef result;
        HandleErrorCode(maths_from_string(expr, &result), e);
        return result;
    }

    Entity::Entity()
        : innerEntityInstance(nullptr, HandleDeleter())
    {
    }

    Entity::Entity(const std::string& expr)
        : Entity(expr.c_str())
    {
    }

    Entity::Entity(const char* expr)
        : Entity(ParseString(expr))
    {
    }

    Entity::Entity(const std::string& expr, ErrorCode& e)
        : Entity(expr.c_str(), e)
    {
    }

    Entity::Entity(const char* expr, ErrorCode& e)
        : Entity(ParseString(expr, e))
    {
    }

    std::string Entity::ToString() const
    {
        char* buff = nullptr;
        HandleErrorCode(entity_to_string(innerEntityInstance.get()->reference, &buff));
        auto res = buff != nullptr ? std::string(buff) : std::string();
        free_string(buff);
        return res;
    }

    std::string Entity::ToString(ErrorCode& ec) const
    {
        char* buff = nullptr;
        HandleErrorCode(entity_to_string(innerEntityInstance.get()->reference, &buff), ec);
        auto res = buff != nullptr ? std::string(buff) : std::string();
        free_string(buff);
        return res;
    }

    Entity Entity::Differentiate(const Entity& var) const
    {
        Internal::EntityRef result;
        HandleErrorCode(entity_differentiate(innerEntityInstance.get()->reference, var.innerEntityInstance.get()->reference, &result));
        return Entity(result);
    }

    Entity Entity::Differentiate(const Entity& var, ErrorCode& ec) const
    {
        Internal::EntityRef result;
        HandleErrorCode(entity_differentiate(innerEntityInstance.get()->reference, var.innerEntityInstance.get()->reference, &result), ec);
        return Entity(result);
    }

    Internal::EntityRef GetHandle(const Entity& e)
    {
        return e.innerEntityInstance.get()->reference;
    }

    Entity CreateByHandle(Internal::EntityRef handle)
    {
        return Entity(handle);
    }

    namespace Internal
    {
        const std::vector<Entity>& EntityInstance::CachedNodes()
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
            const auto& res = nodes.GetValue(lambda, reference);
            return res;
        }
    }
}