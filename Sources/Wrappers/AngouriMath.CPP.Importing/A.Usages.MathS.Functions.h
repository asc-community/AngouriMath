/* 
 * Copyright (c) 2019-2021 Angouri.
 * AngouriMath is licensed under MIT. 
 * Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
 * Website: https://am.angouri.org.
 */

// This file is auto-generated.

#pragma once

#include "AngouriMath.CPP.h"
#include "A.Imports.MathS.Functions.h"

namespace AngouriMath
{
    Entity Sin(const Entity& arg0)
    {
        EntityRef res;
        auto error = math_s_sin(arg0.Handle(), &res);
        return Entity(res);
    }
    Entity Cos(const Entity& arg0)
    {
        EntityRef res;
        auto error = math_s_cos(arg0.Handle(), &res);
        return Entity(res);
    }
    Entity Sec(const Entity& arg0)
    {
        EntityRef res;
        auto error = math_s_sec(arg0.Handle(), &res);
        return Entity(res);
    }
    Entity Cosec(const Entity& arg0)
    {
        EntityRef res;
        auto error = math_s_cosec(arg0.Handle(), &res);
        return Entity(res);
    }
    Entity Log(const Entity& arg0, const Entity& arg1)
    {
        EntityRef res;
        auto error = math_s_log(arg0.Handle(), arg1.Handle(), &res);
        return Entity(res);
    }
    Entity Pow(const Entity& arg0, const Entity& arg1)
    {
        EntityRef res;
        auto error = math_s_pow(arg0.Handle(), arg1.Handle(), &res);
        return Entity(res);
    }
    Entity Sqrt(const Entity& arg0)
    {
        EntityRef res;
        auto error = math_s_sqrt(arg0.Handle(), &res);
        return Entity(res);
    }
    Entity Cbrt(const Entity& arg0)
    {
        EntityRef res;
        auto error = math_s_cbrt(arg0.Handle(), &res);
        return Entity(res);
    }
    Entity Sqr(const Entity& arg0)
    {
        EntityRef res;
        auto error = math_s_sqr(arg0.Handle(), &res);
        return Entity(res);
    }
    Entity Tan(const Entity& arg0)
    {
        EntityRef res;
        auto error = math_s_tan(arg0.Handle(), &res);
        return Entity(res);
    }
    Entity Cotan(const Entity& arg0)
    {
        EntityRef res;
        auto error = math_s_cotan(arg0.Handle(), &res);
        return Entity(res);
    }
    Entity Arcsin(const Entity& arg0)
    {
        EntityRef res;
        auto error = math_s_arcsin(arg0.Handle(), &res);
        return Entity(res);
    }
    Entity Arccos(const Entity& arg0)
    {
        EntityRef res;
        auto error = math_s_arccos(arg0.Handle(), &res);
        return Entity(res);
    }
    Entity Arctan(const Entity& arg0)
    {
        EntityRef res;
        auto error = math_s_arctan(arg0.Handle(), &res);
        return Entity(res);
    }
    Entity Arccotan(const Entity& arg0)
    {
        EntityRef res;
        auto error = math_s_arccotan(arg0.Handle(), &res);
        return Entity(res);
    }
    Entity Arcsec(const Entity& arg0)
    {
        EntityRef res;
        auto error = math_s_arcsec(arg0.Handle(), &res);
        return Entity(res);
    }
    Entity Arccosec(const Entity& arg0)
    {
        EntityRef res;
        auto error = math_s_arccosec(arg0.Handle(), &res);
        return Entity(res);
    }
    Entity Ln(const Entity& arg0)
    {
        EntityRef res;
        auto error = math_s_ln(arg0.Handle(), &res);
        return Entity(res);
    }
    Entity Factorial(const Entity& arg0)
    {
        EntityRef res;
        auto error = math_s_factorial(arg0.Handle(), &res);
        return Entity(res);
    }
    Entity Gamma(const Entity& arg0)
    {
        EntityRef res;
        auto error = math_s_gamma(arg0.Handle(), &res);
        return Entity(res);
    }
    Entity Signum(const Entity& arg0)
    {
        EntityRef res;
        auto error = math_s_signum(arg0.Handle(), &res);
        return Entity(res);
    }
    Entity Abs(const Entity& arg0)
    {
        EntityRef res;
        auto error = math_s_abs(arg0.Handle(), &res);
        return Entity(res);
    }
    Entity Negation(const Entity& arg0)
    {
        EntityRef res;
        auto error = math_s_negation(arg0.Handle(), &res);
        return Entity(res);
    }
    Entity Provided(const Entity& arg0, const Entity& arg1)
    {
        EntityRef res;
        auto error = math_s_provided(arg0.Handle(), arg1.Handle(), &res);
        return Entity(res);
    }

}