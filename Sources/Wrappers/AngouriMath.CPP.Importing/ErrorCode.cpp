#include "AmgouriMathException.h"
#include <cassert>
#include "Imports.h"

namespace AngouriMath::Internal
{
    void DeleteNativeErrorCode(NativeErrorCode nec)
    {
        (void)free_error_code(nec);
    }

    void HandleErrorCode(NativeErrorCode nec)
    {
        if (nec.name != nullptr)
        {
            auto ec = ErrorCode(
                nec.name != nullptr ? nec.name : "",
                nec.message != nullptr ? nec.message : "",
                nec.stackTrace != nullptr ? nec.stackTrace : ""
            );
            DeleteNativeErrorCode(nec);
            throw AngouriMathException(ec);
        }
    }

    void HandleErrorCode(NativeErrorCode nec, ErrorCode& ec)
    {
        ec = ErrorCode(
            nec.name != nullptr ? nec.name : "",
            nec.message != nullptr ? nec.message : "",
            nec.stackTrace != nullptr ? nec.stackTrace : ""
        );
        DeleteNativeErrorCode(nec);
    }
}
