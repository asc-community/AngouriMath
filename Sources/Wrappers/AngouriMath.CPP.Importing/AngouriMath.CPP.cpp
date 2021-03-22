// AngouriMath.CPP.cpp : Defines the entry point for the application.
//

#include "AngouriMath.CPP.h"
#include "ErrorCode.h"
#include <iostream>
#include "imports.h"

void throw_if_needed(const ErrorCode& error);

_EntityRefWrapper::~_EntityRefWrapper()
{
    Imports::free_entity(handle);
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
    Imports::EntityOut resPtr = &res;
    auto error = Imports::entity_differentiate(this->handle(), var.handle(), resPtr);
    throw_if_needed(error);
    return Entity(res);
}

Entity::Entity(const std::string& str)
{
    auto newStr = new char[str.size() + 1];
    memcpy(newStr, &str[0], str.size());
    newStr[str.size()] = 0;
    EntityRef res;
    Imports::EntityOut newHandle = &res;
    auto error = Imports::maths_from_string(newStr, newHandle);
    throw_if_needed(error);
    set_handle(res);
}

std::string Entity::to_string()
{
    char* str;
    Imports::StringOut res = &str;
    auto error = Imports::entity_to_string(handle(), res);
    throw_if_needed(error);
    return std::string(*res);
}


void throw_if_needed(const ErrorCode& error)
{
    if (!error.is_ok())
        throw std::exception(error.name());
}
