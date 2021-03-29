#pragma once

#include "TypeAliases.h"
#include <functional>

namespace AngouriMath::Internal
{
    template<typename T>
    class FieldCache
    {
    private:
        T value;
        bool isValid = false;
    public:
        // TODO: make thread-safe
        T GetValue(std::function<T(EntityRef)> factory, EntityRef ref)
        {
            if (!isValid)
            {
                value = factory(ref);
                isValid = true;
            }
            return value;
        }

        FieldCache() { }
    };
}