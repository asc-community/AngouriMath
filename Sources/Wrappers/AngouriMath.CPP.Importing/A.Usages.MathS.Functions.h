//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

// This file is auto-generated.

#pragma once

#include "AngouriMath.h"
#include "A.Imports.MathS.Functions.h"

namespace AngouriMath
{
    Entity Sin(const Entity& arg0)
    {
        EntityRef res;
        HandleErrorCode(math_s_sin(GetHandle(arg0), &res));
        return CreateByHandle(res);
    }

    Entity Sin(const Entity& arg0, ErrorCode& e)
    {
        EntityRef res;
        HandleErrorCode(math_s_sin(GetHandle(arg0), &res), e);
        return CreateByHandle(res);
    }
    
    Entity Cos(const Entity& arg0)
    {
        EntityRef res;
        HandleErrorCode(math_s_cos(GetHandle(arg0), &res));
        return CreateByHandle(res);
    }

    Entity Cos(const Entity& arg0, ErrorCode& e)
    {
        EntityRef res;
        HandleErrorCode(math_s_cos(GetHandle(arg0), &res), e);
        return CreateByHandle(res);
    }
    
    Entity Sec(const Entity& arg0)
    {
        EntityRef res;
        HandleErrorCode(math_s_sec(GetHandle(arg0), &res));
        return CreateByHandle(res);
    }

    Entity Sec(const Entity& arg0, ErrorCode& e)
    {
        EntityRef res;
        HandleErrorCode(math_s_sec(GetHandle(arg0), &res), e);
        return CreateByHandle(res);
    }
    
    Entity Cosec(const Entity& arg0)
    {
        EntityRef res;
        HandleErrorCode(math_s_cosec(GetHandle(arg0), &res));
        return CreateByHandle(res);
    }

    Entity Cosec(const Entity& arg0, ErrorCode& e)
    {
        EntityRef res;
        HandleErrorCode(math_s_cosec(GetHandle(arg0), &res), e);
        return CreateByHandle(res);
    }
    
    Entity Log(const Entity& arg0, const Entity& arg1)
    {
        EntityRef res;
        HandleErrorCode(math_s_log(GetHandle(arg0), GetHandle(arg1), &res));
        return CreateByHandle(res);
    }

    Entity Log(const Entity& arg0, const Entity& arg1, ErrorCode& e)
    {
        EntityRef res;
        HandleErrorCode(math_s_log(GetHandle(arg0), GetHandle(arg1), &res), e);
        return CreateByHandle(res);
    }
    
    Entity Pow(const Entity& arg0, const Entity& arg1)
    {
        EntityRef res;
        HandleErrorCode(math_s_pow(GetHandle(arg0), GetHandle(arg1), &res));
        return CreateByHandle(res);
    }

    Entity Pow(const Entity& arg0, const Entity& arg1, ErrorCode& e)
    {
        EntityRef res;
        HandleErrorCode(math_s_pow(GetHandle(arg0), GetHandle(arg1), &res), e);
        return CreateByHandle(res);
    }
    
    Entity Sqrt(const Entity& arg0)
    {
        EntityRef res;
        HandleErrorCode(math_s_sqrt(GetHandle(arg0), &res));
        return CreateByHandle(res);
    }

    Entity Sqrt(const Entity& arg0, ErrorCode& e)
    {
        EntityRef res;
        HandleErrorCode(math_s_sqrt(GetHandle(arg0), &res), e);
        return CreateByHandle(res);
    }
    
    Entity Cbrt(const Entity& arg0)
    {
        EntityRef res;
        HandleErrorCode(math_s_cbrt(GetHandle(arg0), &res));
        return CreateByHandle(res);
    }

    Entity Cbrt(const Entity& arg0, ErrorCode& e)
    {
        EntityRef res;
        HandleErrorCode(math_s_cbrt(GetHandle(arg0), &res), e);
        return CreateByHandle(res);
    }
    
    Entity Sqr(const Entity& arg0)
    {
        EntityRef res;
        HandleErrorCode(math_s_sqr(GetHandle(arg0), &res));
        return CreateByHandle(res);
    }

    Entity Sqr(const Entity& arg0, ErrorCode& e)
    {
        EntityRef res;
        HandleErrorCode(math_s_sqr(GetHandle(arg0), &res), e);
        return CreateByHandle(res);
    }
    
    Entity Tan(const Entity& arg0)
    {
        EntityRef res;
        HandleErrorCode(math_s_tan(GetHandle(arg0), &res));
        return CreateByHandle(res);
    }

    Entity Tan(const Entity& arg0, ErrorCode& e)
    {
        EntityRef res;
        HandleErrorCode(math_s_tan(GetHandle(arg0), &res), e);
        return CreateByHandle(res);
    }
    
    Entity Cotan(const Entity& arg0)
    {
        EntityRef res;
        HandleErrorCode(math_s_cotan(GetHandle(arg0), &res));
        return CreateByHandle(res);
    }

    Entity Cotan(const Entity& arg0, ErrorCode& e)
    {
        EntityRef res;
        HandleErrorCode(math_s_cotan(GetHandle(arg0), &res), e);
        return CreateByHandle(res);
    }
    
    Entity Arcsin(const Entity& arg0)
    {
        EntityRef res;
        HandleErrorCode(math_s_arcsin(GetHandle(arg0), &res));
        return CreateByHandle(res);
    }

    Entity Arcsin(const Entity& arg0, ErrorCode& e)
    {
        EntityRef res;
        HandleErrorCode(math_s_arcsin(GetHandle(arg0), &res), e);
        return CreateByHandle(res);
    }
    
    Entity Arccos(const Entity& arg0)
    {
        EntityRef res;
        HandleErrorCode(math_s_arccos(GetHandle(arg0), &res));
        return CreateByHandle(res);
    }

    Entity Arccos(const Entity& arg0, ErrorCode& e)
    {
        EntityRef res;
        HandleErrorCode(math_s_arccos(GetHandle(arg0), &res), e);
        return CreateByHandle(res);
    }
    
    Entity Arctan(const Entity& arg0)
    {
        EntityRef res;
        HandleErrorCode(math_s_arctan(GetHandle(arg0), &res));
        return CreateByHandle(res);
    }

    Entity Arctan(const Entity& arg0, ErrorCode& e)
    {
        EntityRef res;
        HandleErrorCode(math_s_arctan(GetHandle(arg0), &res), e);
        return CreateByHandle(res);
    }
    
    Entity Arccotan(const Entity& arg0)
    {
        EntityRef res;
        HandleErrorCode(math_s_arccotan(GetHandle(arg0), &res));
        return CreateByHandle(res);
    }

    Entity Arccotan(const Entity& arg0, ErrorCode& e)
    {
        EntityRef res;
        HandleErrorCode(math_s_arccotan(GetHandle(arg0), &res), e);
        return CreateByHandle(res);
    }
    
    Entity Arcsec(const Entity& arg0)
    {
        EntityRef res;
        HandleErrorCode(math_s_arcsec(GetHandle(arg0), &res));
        return CreateByHandle(res);
    }

    Entity Arcsec(const Entity& arg0, ErrorCode& e)
    {
        EntityRef res;
        HandleErrorCode(math_s_arcsec(GetHandle(arg0), &res), e);
        return CreateByHandle(res);
    }
    
    Entity Arccosec(const Entity& arg0)
    {
        EntityRef res;
        HandleErrorCode(math_s_arccosec(GetHandle(arg0), &res));
        return CreateByHandle(res);
    }

    Entity Arccosec(const Entity& arg0, ErrorCode& e)
    {
        EntityRef res;
        HandleErrorCode(math_s_arccosec(GetHandle(arg0), &res), e);
        return CreateByHandle(res);
    }
    
    Entity Ln(const Entity& arg0)
    {
        EntityRef res;
        HandleErrorCode(math_s_ln(GetHandle(arg0), &res));
        return CreateByHandle(res);
    }

    Entity Ln(const Entity& arg0, ErrorCode& e)
    {
        EntityRef res;
        HandleErrorCode(math_s_ln(GetHandle(arg0), &res), e);
        return CreateByHandle(res);
    }
    
    Entity Factorial(const Entity& arg0)
    {
        EntityRef res;
        HandleErrorCode(math_s_factorial(GetHandle(arg0), &res));
        return CreateByHandle(res);
    }

    Entity Factorial(const Entity& arg0, ErrorCode& e)
    {
        EntityRef res;
        HandleErrorCode(math_s_factorial(GetHandle(arg0), &res), e);
        return CreateByHandle(res);
    }
    
    Entity Gamma(const Entity& arg0)
    {
        EntityRef res;
        HandleErrorCode(math_s_gamma(GetHandle(arg0), &res));
        return CreateByHandle(res);
    }

    Entity Gamma(const Entity& arg0, ErrorCode& e)
    {
        EntityRef res;
        HandleErrorCode(math_s_gamma(GetHandle(arg0), &res), e);
        return CreateByHandle(res);
    }
    
    Entity Signum(const Entity& arg0)
    {
        EntityRef res;
        HandleErrorCode(math_s_signum(GetHandle(arg0), &res));
        return CreateByHandle(res);
    }

    Entity Signum(const Entity& arg0, ErrorCode& e)
    {
        EntityRef res;
        HandleErrorCode(math_s_signum(GetHandle(arg0), &res), e);
        return CreateByHandle(res);
    }
    
    Entity Abs(const Entity& arg0)
    {
        EntityRef res;
        HandleErrorCode(math_s_abs(GetHandle(arg0), &res));
        return CreateByHandle(res);
    }

    Entity Abs(const Entity& arg0, ErrorCode& e)
    {
        EntityRef res;
        HandleErrorCode(math_s_abs(GetHandle(arg0), &res), e);
        return CreateByHandle(res);
    }
    
    Entity Negation(const Entity& arg0)
    {
        EntityRef res;
        HandleErrorCode(math_s_negation(GetHandle(arg0), &res));
        return CreateByHandle(res);
    }

    Entity Negation(const Entity& arg0, ErrorCode& e)
    {
        EntityRef res;
        HandleErrorCode(math_s_negation(GetHandle(arg0), &res), e);
        return CreateByHandle(res);
    }
    
    Entity Provided(const Entity& arg0, const Entity& arg1)
    {
        EntityRef res;
        HandleErrorCode(math_s_provided(GetHandle(arg0), GetHandle(arg1), &res));
        return CreateByHandle(res);
    }

    Entity Provided(const Entity& arg0, const Entity& arg1, ErrorCode& e)
    {
        EntityRef res;
        HandleErrorCode(math_s_provided(GetHandle(arg0), GetHandle(arg1), &res), e);
        return CreateByHandle(res);
    }
    

}