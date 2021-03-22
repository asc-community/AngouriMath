/*
 * Copyright (c) 2019-2021 Angouri.
 * AngouriMath is licensed under MIT.
 * Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
 * Website: https://am.angouri.org.
 */

#include "AngouriMath.CPP.h"
#include "Imports.h"

namespace AngouriMath
{
    struct HandleDeleter
    {
        void operator()(const Internal::EntityRef* handle)
        {
            if (handle != nullptr)
            {
                auto error = free_entity(*handle);
                delete handle;
            }
        }
    };

    Entity::Entity(Internal::EntityRef handle)
        : handle(new Internal::EntityRef(handle), HandleDeleter())
    {
        *this->handle = handle;
    }

    Internal::EntityRef ParseString(const char* expr)
    {
        Internal::EntityRef result;
        // TODO handle errors
        auto error = maths_from_string(expr, &result);
        return result;
    }

    Entity::Entity()
        : handle(nullptr, HandleDeleter())
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

    std::string Entity::ToString() const
    {
        char* buff = nullptr;
        auto error = entity_to_string(*this->handle, &buff);
        
        return buff != nullptr ? std::string(buff) : std::string();
    }

    Entity Entity::Differentiate(const Entity& var) const
    {
        Internal::EntityRef result;
        auto error = entity_differentiate(*this->handle, *var.handle, &result);
        return Entity(result);
    }
}