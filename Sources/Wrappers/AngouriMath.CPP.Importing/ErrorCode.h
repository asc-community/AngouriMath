#pragma once

#include <string>

namespace AngouriMath
{
    struct ErrorCode
    {
    public:
        ErrorCode(std::string name, std::string stackTrace)
            : name(name), stackTrace(stackTrace) { }

        bool IsOk() const { return this->name.empty(); }
        const std::string& Name() const { return this->name; }
        const std::string& StackTrace() const { return this->stackTrace; }

    private:
        std::string name;
        std::string stackTrace;
    };
}

namespace std
{
    inline std::string to_string(AngouriMath::ErrorCode e)
    {
        return e.Name();
    }
}