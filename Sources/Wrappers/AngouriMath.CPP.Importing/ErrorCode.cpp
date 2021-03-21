#include "ErrorCode.h"

ErrorCode::~ErrorCode()
{
    delete name;
    delete stackTrace;
}