// AngouriMath.CPP.cpp : Defines the entry point for the application.
//


#include "AngouriMath.CPP.h"
#include "ErrorCode.h"
#include <iostream>

void throw_if_needed(const ErrorCode& error);

_EntityRefWrapper::~_EntityRefWrapper()
{
    static e2 cache = nullptr;
    if (cache == nullptr)
        cache = (e2)import(AM_PATH, "free_entity");
    cache(handle);
}

Entity::Entity(EntityRef __handle)
{
    set_handle(__handle);
}

EntityRef Entity::handle()
{
    return handle_ptr.get()->handle;
}

void Entity::set_handle(EntityRef ref)
{
    handle_ptr = std::shared_ptr< _EntityRefWrapper>(new _EntityRefWrapper(ref));    
}

Entity Entity::diff(Entity var)
{
    static ee2e cache = nullptr;
    if (cache == nullptr)
        cache = (ee2e)import(AM_PATH, "diff");
    EntityRef res;
    auto error = cache(this->handle(), var.handle(), res);
    throw_if_needed(error);
    return Entity(res);
}

Entity::Entity(const std::string& str)
{
    static s2e cache = nullptr;
    if (cache == nullptr)
        cache = (s2e)import(AM_PATH, "parse");
    auto newStr = new char[str.size() + 1];
    memcpy(newStr, &str[0], str.size());
    newStr[str.size()] = 0;
    EntityRef newHandle;
    auto error = cache(newStr, newHandle);
    throw_if_needed(error);
    set_handle(newHandle);
}

std::string Entity::to_string()
{
    static e2s cache = nullptr;
    if (cache == nullptr)
        cache = (e2s)import(AM_PATH, "entity_to_string");
    char* res = nullptr;
    auto error = cache(handle(), res);
    throw_if_needed(error);
    return std::string(res);
}


void throw_if_needed(const ErrorCode& error)
{
    if (!error.is_ok())
        throw std::exception(error.name());
}
