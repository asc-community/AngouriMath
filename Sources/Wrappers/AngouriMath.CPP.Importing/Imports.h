#pragma once

#include "TypeAliases.h"

using namespace AngouriMath::Internal;

extern "C"
{
# if defined(_MSC_VER) && !defined(__clang__) // clang on MSVC is a thing
#  define DLL_CODE __declspec(dllimport)
# else
#  define DLL_CODE // nothing, you don't need it
# endif
    DLL_CODE NativeErrorCode free_entity(EntityRef);
    DLL_CODE NativeErrorCode free_native_array(NativeArray);
    DLL_CODE NativeErrorCode free_error_code(NativeErrorCode);
    DLL_CODE NativeErrorCode free_string(String);

    DLL_CODE NativeErrorCode entity_to_string(EntityRef, StringOut);
    DLL_CODE NativeErrorCode entity_latexise(EntityRef, StringOut);
    DLL_CODE NativeErrorCode maths_from_string(String, EntityOut);

    DLL_CODE NativeErrorCode entity_differentiate(EntityRef, EntityRef, EntityOut);
    DLL_CODE NativeErrorCode entity_integrate(EntityRef, EntityRef, EntityOut);
    DLL_CODE NativeErrorCode entity_solve(EntityRef, EntityRef, EntityOut);
    DLL_CODE NativeErrorCode entity_solve_equation(EntityRef, EntityRef, EntityOut);
    DLL_CODE NativeErrorCode entity_integrate(EntityRef, EntityRef, EntityOut);
    DLL_CODE NativeErrorCode entity_limit(EntityRef, EntityRef, EntityRef, ApproachFrom, EntityOut);
    DLL_CODE NativeErrorCode entity_alternate(EntityRef, NativeArray*);
    DLL_CODE NativeErrorCode entity_simplify(EntityRef, EntityOut);
    DLL_CODE NativeErrorCode entity_evaled(EntityRef, EntityOut);
    DLL_CODE NativeErrorCode entity_inner_simplified(EntityRef, EntityOut);
    DLL_CODE NativeErrorCode entity_to_long(EntityRef, int64_t*);
    DLL_CODE NativeErrorCode entity_to_rational(EntityRef, LongTuple*);
    DLL_CODE NativeErrorCode entity_to_double(EntityRef, double*);
    DLL_CODE NativeErrorCode entity_to_complex(EntityRef, DoubleTuple*);

    DLL_CODE NativeErrorCode op_entity_add(EntityRef, EntityRef, EntityOut);
    DLL_CODE NativeErrorCode op_entity_sub(EntityRef, EntityRef, EntityOut);
    DLL_CODE NativeErrorCode op_entity_mul(EntityRef, EntityRef, EntityOut);
    DLL_CODE NativeErrorCode op_entity_div(EntityRef, EntityRef, EntityOut);

    DLL_CODE NativeErrorCode entity_nodes(EntityRef, NativeArray*);
    DLL_CODE NativeErrorCode entity_vars(EntityRef, NativeArray*);
    DLL_CODE NativeErrorCode entity_vars_and_constants(EntityRef, NativeArray*);
    DLL_CODE NativeErrorCode entity_direct_children(EntityRef, NativeArray*);
}