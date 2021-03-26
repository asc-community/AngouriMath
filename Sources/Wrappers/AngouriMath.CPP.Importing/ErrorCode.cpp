#include "AmgouriMathException.h"
#include <cassert>
#include "imports.h"

namespace AngouriMath::Internal
{
    void DeleteNativeErrorCode(NativeErrorCode nec)
    {
        (void)free_error_code(nec);
    }

    void HandleErrorCode(NativeErrorCode nec)
    {
        #if defined(ANGOURIMATH_DISABLE_EXCEPTIONS)
        assert(false);
        #else
        if (nec.Name != nullptr)
        {
            auto ec = ErrorCode(
                nec.Name != nullptr ? nec.Name : "",
                nec.Message != nullptr ? nec.Message : "",
                nec.StackTrace != nullptr ? nec.StackTrace : ""
            );
            DeleteNativeErrorCode(nec);
            throw AngouriMathException(ec);
        }
        #endif
    }

    void HandleErrorCode(NativeErrorCode nec, ErrorCode& ec)
    {
        ec = ErrorCode(
            nec.Name != nullptr ? nec.Name : "",
            nec.Message != nullptr ? nec.Message : "",
            nec.StackTrace != nullptr ? nec.StackTrace : ""
        );
        DeleteNativeErrorCode(nec);
    }
}
