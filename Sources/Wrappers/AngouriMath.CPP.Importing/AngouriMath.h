#pragma once

#include "TypeAliases.h"
#include "ErrorCode.h"
#include <memory>
#include <string>
#include <ostream>
#include <vector>

namespace AngouriMath
{
    class Entity
    {
    public:
        Entity();
        Entity(const std::string& expr);
        Entity(const char* expr);
        Entity(const std::string& expr, ErrorCode& e);
        Entity(const char* expr, ErrorCode& e);

        std::string ToString() const;
        std::string ToString(ErrorCode& ec) const;

        Entity Differentiate(const Entity& var) const;
        Entity Differentiate(const Entity& var, ErrorCode& ec) const;

        // TODO: to be rewritten!
        std::vector<Entity> Nodes() const;

        friend Internal::EntityRef GetHandle(const Entity& e);
        friend Entity CreateByHandle(Internal::EntityRef handle);
    private:
        explicit Entity(Internal::EntityRef handle);
        
        std::shared_ptr<Internal::EntityRef> handle;
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