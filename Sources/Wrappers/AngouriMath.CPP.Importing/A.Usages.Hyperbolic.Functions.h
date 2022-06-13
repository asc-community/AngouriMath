//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

// This file is auto-generated.

#pragma once

#include "AngouriMath.h"
#include "A.Imports.Hyperbolic.Functions.h"

namespace AngouriMath
{
    Entity Sinh(const Entity& arg0)
    {
        EntityRef res;
        HandleErrorCode(hyperbolic_sinh(GetHandle(arg0), &res));
        return CreateByHandle(res);
    }

    Entity Sinh(const Entity& arg0, ErrorCode& e)
    {
        EntityRef res;
        HandleErrorCode(hyperbolic_sinh(GetHandle(arg0), &res), e);
        return CreateByHandle(res);
    }
    
    Entity Cosh(const Entity& arg0)
    {
        EntityRef res;
        HandleErrorCode(hyperbolic_cosh(GetHandle(arg0), &res));
        return CreateByHandle(res);
    }

    Entity Cosh(const Entity& arg0, ErrorCode& e)
    {
        EntityRef res;
        HandleErrorCode(hyperbolic_cosh(GetHandle(arg0), &res), e);
        return CreateByHandle(res);
    }
    
    Entity Tanh(const Entity& arg0)
    {
        EntityRef res;
        HandleErrorCode(hyperbolic_tanh(GetHandle(arg0), &res));
        return CreateByHandle(res);
    }

    Entity Tanh(const Entity& arg0, ErrorCode& e)
    {
        EntityRef res;
        HandleErrorCode(hyperbolic_tanh(GetHandle(arg0), &res), e);
        return CreateByHandle(res);
    }
    
    Entity Cotanh(const Entity& arg0)
    {
        EntityRef res;
        HandleErrorCode(hyperbolic_cotanh(GetHandle(arg0), &res));
        return CreateByHandle(res);
    }

    Entity Cotanh(const Entity& arg0, ErrorCode& e)
    {
        EntityRef res;
        HandleErrorCode(hyperbolic_cotanh(GetHandle(arg0), &res), e);
        return CreateByHandle(res);
    }
    
    Entity Sech(const Entity& arg0)
    {
        EntityRef res;
        HandleErrorCode(hyperbolic_sech(GetHandle(arg0), &res));
        return CreateByHandle(res);
    }

    Entity Sech(const Entity& arg0, ErrorCode& e)
    {
        EntityRef res;
        HandleErrorCode(hyperbolic_sech(GetHandle(arg0), &res), e);
        return CreateByHandle(res);
    }
    
    Entity Cosech(const Entity& arg0)
    {
        EntityRef res;
        HandleErrorCode(hyperbolic_cosech(GetHandle(arg0), &res));
        return CreateByHandle(res);
    }

    Entity Cosech(const Entity& arg0, ErrorCode& e)
    {
        EntityRef res;
        HandleErrorCode(hyperbolic_cosech(GetHandle(arg0), &res), e);
        return CreateByHandle(res);
    }
    
    Entity Arsinh(const Entity& arg0)
    {
        EntityRef res;
        HandleErrorCode(hyperbolic_arsinh(GetHandle(arg0), &res));
        return CreateByHandle(res);
    }

    Entity Arsinh(const Entity& arg0, ErrorCode& e)
    {
        EntityRef res;
        HandleErrorCode(hyperbolic_arsinh(GetHandle(arg0), &res), e);
        return CreateByHandle(res);
    }
    
    Entity Arcosh(const Entity& arg0)
    {
        EntityRef res;
        HandleErrorCode(hyperbolic_arcosh(GetHandle(arg0), &res));
        return CreateByHandle(res);
    }

    Entity Arcosh(const Entity& arg0, ErrorCode& e)
    {
        EntityRef res;
        HandleErrorCode(hyperbolic_arcosh(GetHandle(arg0), &res), e);
        return CreateByHandle(res);
    }
    
    Entity Artanh(const Entity& arg0)
    {
        EntityRef res;
        HandleErrorCode(hyperbolic_artanh(GetHandle(arg0), &res));
        return CreateByHandle(res);
    }

    Entity Artanh(const Entity& arg0, ErrorCode& e)
    {
        EntityRef res;
        HandleErrorCode(hyperbolic_artanh(GetHandle(arg0), &res), e);
        return CreateByHandle(res);
    }
    
    Entity Arcotanh(const Entity& arg0)
    {
        EntityRef res;
        HandleErrorCode(hyperbolic_arcotanh(GetHandle(arg0), &res));
        return CreateByHandle(res);
    }

    Entity Arcotanh(const Entity& arg0, ErrorCode& e)
    {
        EntityRef res;
        HandleErrorCode(hyperbolic_arcotanh(GetHandle(arg0), &res), e);
        return CreateByHandle(res);
    }
    
    Entity Arsech(const Entity& arg0)
    {
        EntityRef res;
        HandleErrorCode(hyperbolic_arsech(GetHandle(arg0), &res));
        return CreateByHandle(res);
    }

    Entity Arsech(const Entity& arg0, ErrorCode& e)
    {
        EntityRef res;
        HandleErrorCode(hyperbolic_arsech(GetHandle(arg0), &res), e);
        return CreateByHandle(res);
    }
    
    Entity Arcosech(const Entity& arg0)
    {
        EntityRef res;
        HandleErrorCode(hyperbolic_arcosech(GetHandle(arg0), &res));
        return CreateByHandle(res);
    }

    Entity Arcosech(const Entity& arg0, ErrorCode& e)
    {
        EntityRef res;
        HandleErrorCode(hyperbolic_arcosech(GetHandle(arg0), &res), e);
        return CreateByHandle(res);
    }
    

}