/*
 * Copyright (c) 2019-2021 Angouri.
 * AngouriMath is licensed under MIT.
 * Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
 * Website: https://am.angouri.org.
 */

#include "AngouriMath.h"
#include "Imports.h"
#include <vector>

namespace AngouriMath
{
    struct HandleDeleter
    {
        void operator()(const Internal::EntityInstance* inner)
        {
            if (inner != nullptr)
            {
                (void)free_entity(inner->ref);
                delete inner;
            }
        }
    };

    Entity::Entity(Internal::EntityRef handle)
        : innerEntityInstance(new Internal::EntityInstance(innerEntityInstance->ref), HandleDeleter())
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
        HandleErrorCode(entity_to_string(innerEntityInstance->ref, &buff));
        auto res = buff != nullptr ? std::string(buff) : std::string();
        free_string(buff);
        return res;
    }

    std::string Entity::ToString(ErrorCode& ec) const
    {
        char* buff = nullptr;
        HandleErrorCode(entity_to_string(innerEntityInstance->ref, &buff), ec);
        auto res = buff != nullptr ? std::string(buff) : std::string();
        free_string(buff);
        return res;
    }

    Entity Entity::Differentiate(const Entity& var) const
    {
        Internal::EntityRef result;
        HandleErrorCode(entity_differentiate(innerEntityInstance->ref, var.innerEntityInstance->ref, &result));
        return Entity(result);
    }

    Entity Entity::Differentiate(const Entity& var, ErrorCode& ec) const
    {
        Internal::EntityRef result;
        HandleErrorCode(entity_differentiate(innerEntityInstance->ref, var.innerEntityInstance->ref, &result), ec);
        return Entity(result);
    }

    Internal::EntityRef GetHandle(const Entity& e)
    {
        return e.innerEntityInstance->ref;
    }

    Entity CreateByHandle(Internal::EntityRef handle)
    {
        return Entity(handle);
    }
}
