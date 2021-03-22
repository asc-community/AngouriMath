namespace Imports
{
    typedef EntityRef* EntityOut;
    typedef char** StringOut;

    typedef char* String;
    typedef int ApproachFrom; // in the outer API, it should be a enum


    extern "C"
    {
        __declspec(dllimport) ErrorCode free_entity(EntityRef);

        __declspec(dllimport) ErrorCode entity_to_string(EntityRef, StringOut);
        __declspec(dllimport) ErrorCode entity_latexise(EntityRef, StringOut);
        __declspec(dllimport) ErrorCode maths_from_string(String, EntityOut);

        __declspec(dllimport) ErrorCode entity_differentiate(EntityRef, EntityRef, EntityOut);
        __declspec(dllimport) ErrorCode entity_integrate(EntityRef, EntityRef, EntityOut);
        __declspec(dllimport) ErrorCode entity_limit(EntityRef, EntityRef, EntityRef, ApproachFrom, EntityOut);

        __declspec(dllimport) ErrorCode op_entity_add(EntityRef, EntityRef, EntityOut);
        __declspec(dllimport) ErrorCode op_entity_sub(EntityRef, EntityRef, EntityOut);
        __declspec(dllimport) ErrorCode op_entity_mul(EntityRef, EntityRef, EntityOut);
        __declspec(dllimport) ErrorCode op_entity_div(EntityRef, EntityRef, EntityOut);
    }
}