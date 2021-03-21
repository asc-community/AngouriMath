#include "ErrorCode.h"

ErrorCode::~ErrorCode()
{
    if (is_ok())
        return;
    // TODO: delete name and stacktrace
    // delete _name;
    // delete _stackTrace;
}

bool ErrorCode::is_ok() const
{
    return _name == nullptr;
}
