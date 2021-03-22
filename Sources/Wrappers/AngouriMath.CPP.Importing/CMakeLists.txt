# CMakeList.txt : CMake project for AngouriMath.CPP, include source and define
# project specific logic here.
#
cmake_minimum_required (VERSION 3.8)

project ("AngouriMath.CPP.Importing")
# Add source to this project's executable.

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# TODO: Add tests and install targets if needed.

set(SOURCES
"AngouriMath.CPP.cpp"
"A.Usages.MathS.Functions.h"
"A.Usages.Hyperbolic.Functions.h"
"A.Imports.MathS.Functions.h"
"A.Imports.Hyperbolic.Functions.h"
)

add_library(${PROJECT_NAME} ${SOURCES} "Imports.h")

target_link_directories(${PROJECT_NAME} PUBLIC "../AngouriMath.CPP.Exporting/bin/x64/release/netstandard2.0/win-x64/native")
target_link_libraries(${PROJECT_NAME} PUBLIC AngouriMath.CPP.Exporting)