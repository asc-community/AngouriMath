#pragma once

#include <cstdint>

namespace AngouriMath::Internal
{
    typedef uint64_t EntityRef;
    typedef EntityRef* EntityOut;
    typedef char** StringOut;

    typedef const char* String;
    typedef int32_t ApproachFrom; // in the outer API, it should be a enum

    typedef struct { int64_t first; int64_t second; } LongTuple;
    typedef struct { double first; double second; } DoubleTuple;

    struct NativeErrorCode
    {
        const char* name;
        const char* message;
        const char* stackTrace;
    };

    struct NativeArray
    {
        int32_t length;
        const EntityRef* refs;
    };
}