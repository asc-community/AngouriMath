/* 
 * Copyright (c) 2019-2021 Angouri.
 * AngouriMath is licensed under MIT. 
 * Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
 * Website: https://am.angouri.org.
 */

// This file is auto-generated.

#include "AngouriMath.CPP.h"
#include "A.Usages.Hyperbolic.Functions.h"

namespace AngouriMath
{
    Entity Sinh(const Entity& arg0)
    {
        EntityRef res;
        auto error = hyperbolic_sinh(arg0, &res);
        return res;
    }
    Entity Cosh(const Entity& arg0)
    {
        EntityRef res;
        auto error = hyperbolic_cosh(arg0, &res);
        return res;
    }
    Entity Tanh(const Entity& arg0)
    {
        EntityRef res;
        auto error = hyperbolic_tanh(arg0, &res);
        return res;
    }
    Entity Cotanh(const Entity& arg0)
    {
        EntityRef res;
        auto error = hyperbolic_cotanh(arg0, &res);
        return res;
    }
    Entity Sech(const Entity& arg0)
    {
        EntityRef res;
        auto error = hyperbolic_sech(arg0, &res);
        return res;
    }
    Entity Cosech(const Entity& arg0)
    {
        EntityRef res;
        auto error = hyperbolic_cosech(arg0, &res);
        return res;
    }
    Entity Arsinh(const Entity& arg0)
    {
        EntityRef res;
        auto error = hyperbolic_arsinh(arg0, &res);
        return res;
    }
    Entity Arcosh(const Entity& arg0)
    {
        EntityRef res;
        auto error = hyperbolic_arcosh(arg0, &res);
        return res;
    }
    Entity Artanh(const Entity& arg0)
    {
        EntityRef res;
        auto error = hyperbolic_artanh(arg0, &res);
        return res;
    }
    Entity Arcotanh(const Entity& arg0)
    {
        EntityRef res;
        auto error = hyperbolic_arcotanh(arg0, &res);
        return res;
    }
    Entity Arsech(const Entity& arg0)
    {
        EntityRef res;
        auto error = hyperbolic_arsech(arg0, &res);
        return res;
    }
    Entity Arcosech(const Entity& arg0)
    {
        EntityRef res;
        auto error = hyperbolic_arcosech(arg0, &res);
        return res;
    }

}