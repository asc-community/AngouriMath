#include "AmgouriMathException.h"
#include <cassert>
#include "imports.h"

namespace AngouriMath::Internal
{
    void DeleteNativeErrorCode(NativeErrorCode nec)
    {
        free_error_code(nec);
    }

    void HandleErrorCode(NativeErrorCode nec)
    {
        #if defined(ANGOURIMATH_DISABLE_EXCEPTIONS)
        assert(false);
        #else
        if (nec.Name != nullptr)
        {
            auto errorCode = ErrorCode(nec.Name, nec.StackTrace);
            DeleteNativeErrorCode(nec);
            throw AngouriMathException(errorCode);
        }
        #endif
    }

    void HandleErrorCode(NativeErrorCode nec, ErrorCode& ec)
    {
        ec = ErrorCode(nec.Name, nec.StackTrace);
        DeleteNativeErrorCode(nec);
    }
}
