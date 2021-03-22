#pragma once

#include <cstdint>

namespace AngouriMath::Internal
{
    typedef uint64_t EntityRef;
    typedef EntityRef* EntityOut;
    typedef char** StringOut;

    typedef const char* String;
    typedef int ApproachFrom; // in the outer API, it should be a enum

    struct NativeErrorCode
    {
        const char* Name;
        const char* StackTrace;
    };
}