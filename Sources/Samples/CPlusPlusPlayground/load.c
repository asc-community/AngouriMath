// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

//On unix make sure to compile using -ldl and -pthread flags.

//Set this value accordingly to your workspace settings
#if defined(_WIN32)
// #define PathToLibrary "D:\\main\\vs_prj\\AngouriMath\\AngouriMath\\Sources\\AngouriMath\\bin\\Release\\netstandard2.0\\win-x64\\native\\AngouriMath.dll"
// #define PathToLibrary "D:\\main\\vs_prj\\nativeaottest\\Test\\bin\\release\\netstandard2.0\\win-x64\\publish\\Test.dll"
#define PathToLibrary "D:\\main\\vs_prj\\AngouriMath\\AngouriMath\\Sources\\AngouriMath\\bin\\x64\\release\\netstandard2.0\\win-x64\\publish\\AngouriMath.dll"
#elif defined(__APPLE__)
#define PathToLibrary "./bin/Debug/net5.0/osx-x64/native/NativeLibrary.dylib"
#else
#define PathToLibrary "./bin/Debug/net5.0/linux-x64/native/NativeLibrary.so"
#endif

#ifdef _WIN32
#include "windows.h"
#define symLoad GetProcAddress
#else
#include "dlfcn.h"
#include <unistd.h>
#define symLoad dlsym
#endif

#include <stdlib.h>
#include <stdio.h>

#ifndef F_OK
#define F_OK    0
#endif

typedef char*(*func)(char*,char*);

func import(char *path, char *funcName);

int main()
{
    // Check if the library file exists
    if (access(PathToLibrary, F_OK) == -1)
    {
        puts("Couldn't find library at the specified path");
        return 0;
    }

    // Sum two integers
    func diff = import(PathToLibrary, "diff");
	char expr[1000];
	char line[100];
	// scanf("%[^\n]", expr);
	// scanf("%[^\n]", line);
	gets(expr); gets(line);
    printf("\nThe diff is %s \n", diff(expr, line));

}

func import(char *path, char *funcName)
{
    // Call sum function defined in C# shared library
    #ifdef _WIN32
        HINSTANCE handle = LoadLibrary(path);
    #else
        void *handle = dlopen(path, RTLD_LAZY);
    #endif

	if ((int)handle == 0)
	{
		printf("\nLast error: %d", GetLastError());
		printf("\nHandle: %d", (int)handle);
		return 0;
	}
	
    
    func MyImport = (func)symLoad(handle, funcName);
	
	return MyImport;
}
