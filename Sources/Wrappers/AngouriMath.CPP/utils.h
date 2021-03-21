#ifdef _WIN32
#include "windows.h"
#define symLoad GetProcAddress
#else
#include "dlfcn.h"
#include <unistd.h>
#define symLoad dlsym
#endif

void* import(char* path, char* funcName)
{
    // Call sum function defined in C# shared library
#ifdef _WIN32
    HINSTANCE handle = LoadLibrary(path);
#else
    void* handle = dlopen(path, RTLD_LAZY);
#endif

    if ((int)handle == 0)
    {
        printf("\nLast error: %d", GetLastError());
        printf("\nHandle: %d", (int)handle);
        return 0;
    }


    void* sym = symLoad(handle, funcName);

    if ((int)sym == 0)
    {
        printf("Quacksdj jksdf panic!!11");
        return 0;
    }

    return sym;
}