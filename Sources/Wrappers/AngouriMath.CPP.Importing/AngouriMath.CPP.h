#pragma once

#include "TypeAliases.h"
#include <memory>
#include <string>
#include <ostream>

namespace AngouriMath
{
    class Entity
    {
    public:        
        Entity();
        Entity(const std::string& expr);
        Entity(const char* expr);

        std::string ToString() const;

        Entity Differentiate(const Entity& var) const;
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