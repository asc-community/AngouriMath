# CMakeList.txt : CMake project for CPlusPlusPlayground, include source and define
# project specific logic here.
#
cmake_minimum_required (VERSION 3.8)

project ("CPlusPlusPlayground")

# Add source to this project's executable.
add_executable (CPlusPlusPlayground "CPlusPlusPlayground.cpp" "CPlusPlusPlayground.h" "../../Wrappers/AngouriMath.CPP/AngouriMath.CPP.h" "../../Wrappers/AngouriMath.CPP/AngouriMath.CPP.cpp")

# TODO: Add tests and install targets if needed.
