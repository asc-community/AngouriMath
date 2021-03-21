#pragma once

#include <stdint.h>
#include <string>
#include <memory>

typedef uint64_t EntityRef;

class _EntityRefWrapper;

class Entity
{
public:
    Entity diff(Entity var);
    Entity(const std::string& str);
    std::string to_string();
private:
    std::shared_ptr < _EntityRefWrapper > handle_ptr;

    EntityRef handle();
    void set_handle(EntityRef new_handle);
    Entity(EntityRef __handle);
};


class _EntityRefWrapper
{
    friend Entity;
public:
    _EntityRefWrapper(EntityRef handle) : handle(handle) { }
    EntityRef handle;
    ~_EntityRefWrapper();
};