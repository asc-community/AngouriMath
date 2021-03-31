#pragma once

#include "TypeAliases.h"

using namespace AngouriMath::Internal;

extern "C"
{
    __declspec(dllimport) NativeErrorCode free_entity(EntityRef);
    __declspec(dllimport) NativeErrorCode free_native_array(NativeArray);
    __declspec(dllimport) NativeErrorCode free_error_code(NativeErrorCode);
    __declspec(dllimport) NativeErrorCode free_string(String);

    __declspec(dllimport) NativeErrorCode entity_to_string(EntityRef, StringOut);
    __declspec(dllimport) NativeErrorCode entity_latexise(EntityRef, StringOut);
    __declspec(dllimport) NativeErrorCode maths_from_string(String, EntityOut);

    __declspec(dllimport) NativeErrorCode entity_differentiate(EntityRef, EntityRef, EntityOut);
    __declspec(dllimport) NativeErrorCode entity_integrate(EntityRef, EntityRef, EntityOut);
    __declspec(dllimport) NativeErrorCode entity_limit(EntityRef, EntityRef, EntityRef, ApproachFrom, EntityOut);

    __declspec(dllimport) NativeErrorCode op_entity_add(EntityRef, EntityRef, EntityOut);
    __declspec(dllimport) NativeErrorCode op_entity_sub(EntityRef, EntityRef, EntityOut);
    __declspec(dllimport) NativeErrorCode op_entity_mul(EntityRef, EntityRef, EntityOut);
    __declspec(dllimport) NativeErrorCode op_entity_div(EntityRef, EntityRef, EntityOut);

    __declspec(dllimport) NativeErrorCode entity_nodes(EntityRef, NativeArray*);
    __declspec(dllimport) NativeErrorCode entity_vars(EntityRef, NativeArray*);
    __declspec(dllimport) NativeErrorCode entity_vars_and_constants(EntityRef, NativeArray*);
}