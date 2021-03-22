# CMakeList.txt : CMake project for AngouriMath.CPP, include source and define
# project specific logic here.
#
cmake_minimum_required (VERSION 3.8)

project ("AngouriMath.CPP.Importing")
# Add source to this project's executable.

# TODO: Add tests and install targets if needed.

set(SOURCES
"AngouriMath.CPP.cpp"
"ErrorCode.cpp"
)

add_library(${PROJECT_NAME} ${SOURCES} "Imports.h")

target_link_directories(${PROJECT_NAME} PUBLIC "../AngouriMath.CPP.Exporting/bin/x64/release/netstandard2.0/win-x64/native")
target_link_libraries(${PROJECT_NAME} PUBLIC AngouriMath.CPP.Exporting)