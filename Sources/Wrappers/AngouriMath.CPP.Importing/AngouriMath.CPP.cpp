// AngouriMath.CPP.cpp : Defines the entry point for the application.
//

#define AM_PATH "D:\\main\\vs_prj\\AngouriMath\\AngouriMath\\Sources\\Wrappers\\AngouriMath.CPP.Exporting\\bin\\x64\\release\\netstandard2.0\\win-x64\\publish\\AngouriMath.CPP.Exporting.dll"

#include "AngouriMath.CPP.h"
#include "utils.h"
#include <iostream>

typedef EntityRef(*ee2e)(EntityRef, EntityRef);
typedef EntityRef(*s2e)(char*);
typedef char*(*e2s)(EntityRef);

Entity Entity::diff(Entity var)
{
    static ee2e cache = nullptr;
    if (cache == nullptr)
        cache = (ee2e)import(AM_PATH, "diff");
    return cache(this->handle, var.handle);
}

Entity::Entity(const std::string& str)
{
    static s2e cache = nullptr;
    if (cache == nullptr)
        cache = (s2e)import(AM_PATH, "parse");
    auto newStr = new char[str.size() + 1];
    memcpy(newStr, &str[0], str.size());
    newStr[str.size()] = 0;
    handle = cache(newStr);
}

std::string Entity::to_string()
{
    static e2s cache = nullptr;
    if (cache == nullptr)
        cache = (e2s)import(AM_PATH, "entity_to_string");
    return std::string(cache(handle));
}
