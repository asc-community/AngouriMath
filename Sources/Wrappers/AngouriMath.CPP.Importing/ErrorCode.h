#pragma once

#include <string>
#include "TypeAliases.h"

namespace AngouriMath
{
    struct ErrorCode
    {
    public:
        ErrorCode() { }

        ErrorCode(std::string name, std::string message, std::string stackTrace)
            : name(std::move(name)), message(std::move(message)), stackTrace(std::move(stackTrace)) { }

        bool IsOk() const { return this->name.empty(); }
        const std::string& Name() const { return this->name; }
        const std::string& Message() const { return this->message; }
        const std::string& StackTrace() const { return this->stackTrace; }
    private:
        std::string name;
        std::string message;
        std::string stackTrace;
    };

    namespace Internal
    {
        void HandleErrorCode(ErrorCode ec);
        void HandleErrorCode(NativeErrorCode nec);
        void HandleErrorCode(NativeErrorCode nec, ErrorCode& ec);
    }
}

namespace std
{
    inline std::string to_string(AngouriMath::ErrorCode e)
    {
        return e.Name();
    }
}