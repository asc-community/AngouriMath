#pragma once

#include <stdint.h>
#include <string>

typedef uint64_t EntityRef;

class Entity
{
public:
    Entity diff(Entity var);
    Entity(const std::string& str);
    std::string to_string();
private:
    EntityRef handle;
    Entity(EntityRef handle) : handle(handle) { }
};
