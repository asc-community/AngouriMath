#include "AmgouriMathException.h"
#include <cassert>

namespace AngouriMath::Internal
{
    void HandleErrorCode(NativeErrorCode nec)
    {
        #if defined(ANGOURIMATH_DISABLE_EXCEPTIONS)
        assert(false);
        #else
        if (nec.Name != nullptr)
            throw AngouriMathException(ErrorCode(nec.Name, nec.StackTrace));
        #endif
    }

    void HandleErrorCode(NativeErrorCode nec, ErrorCode& ec)
    {
        ec = ErrorCode(nec.Name, nec.StackTrace);
    }
}
