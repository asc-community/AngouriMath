namespace Imports
{
    typedef ErrorCode(ee2e)(EntityRef, EntityRef, EntityRef&);
    typedef ErrorCode(s2e)(char*, EntityRef&);
    typedef ErrorCode(e2s)(EntityRef, char*&);
    typedef ErrorCode(e2)(EntityRef);
    typedef ErrorCode(eeei2e)(EntityRef, EntityRef, EntityRef, int, EntityRef&);


    extern "C"
    {
        __declspec(dllimport) e2 free_entity;

        __declspec(dllimport) e2s entity_to_string;
        __declspec(dllimport) e2s entity_latexise;
        __declspec(dllimport) s2e maths_from_string;

        __declspec(dllimport) ee2e entity_differentiate;
        __declspec(dllimport) ee2e entity_integrate;
        __declspec(dllimport) eeei2e entity_limit;
    }
}