// AngouriMath.CPP.cpp : Defines the entry point for the application.
//

#include "AngouriMath.CPP.h"
#include "ErrorCode.h"
#include <iostream>

typedef ErrorCode(ee2e)(EntityRef, EntityRef, EntityRef&);
typedef ErrorCode(s2e)(char*, EntityRef&);
typedef ErrorCode(e2s)(EntityRef, char*&);
typedef ErrorCode(e2)(EntityRef);

extern "C"
{
    __declspec(dllimport) e2 free_entity;
    __declspec(dllimport) e2s entity_to_string;
    __declspec(dllimport) ee2e diff;
    __declspec(dllimport) s2e parse;

    __declspec(dllimport) int add(int, int);
}

void throw_if_needed(const ErrorCode& error);

_EntityRefWrapper::~_EntityRefWrapper()
{
    int x = add(0, 1);
    free_entity(handle);
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
    EntityRef res;
    auto error = ::diff(this->handle(), var.handle(), res);
    throw_if_needed(error);
    return Entity(res);
}

Entity::Entity(const std::string& str)
{
    auto newStr = new char[str.size() + 1];
    memcpy(newStr, &str[0], str.size());
    newStr[str.size()] = 0;
    EntityRef newHandle;
    auto error = parse(newStr, newHandle);
    throw_if_needed(error);
    set_handle(newHandle);
}

std::string Entity::to_string()
{
    char* res = nullptr;
    auto error = entity_to_string(handle(), res);
    throw_if_needed(error);
    return std::string(res);
}


void throw_if_needed(const ErrorCode& error)
{
    if (!error.is_ok())
        throw std::exception(error.name());
}
