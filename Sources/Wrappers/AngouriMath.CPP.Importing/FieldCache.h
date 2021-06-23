#pragma once

#include "TypeAliases.h"
#include <optional>

namespace AngouriMath::Internal
{
    template<typename T>
    class FieldCache
    {
    private:
        std::optional<T> cached;
    public:
        // TODO: make thread-safe
        template<typename Factory>
        const T& GetValue(Factory&& factory, EntityRef ref)
        {
            if (!cached.has_value())
                cached = factory(ref);
            return this->cached.value();
        }
    };
}