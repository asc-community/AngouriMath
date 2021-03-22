# CMakeList.txt : CMake project for CPlusPlusPlayground, include source and define
# project specific logic here.
#
cmake_minimum_required (VERSION 3.8)

project ("CPlusPlusPlayground")

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

set(ANGOURIMATH_CPP_IMPORTING_PATH "../../Wrappers/AngouriMath.CPP.Importing")

add_subdirectory(${ANGOURIMATH_CPP_IMPORTING_PATH} ${CMAKE_CURRENT_BINARY_DIR}/AngouriMath.CPP.Importing)

set(SOURCES "CPlusPlusPlayground.cpp")

add_executable(${PROJECT_NAME} ${SOURCES})

target_link_libraries(${PROJECT_NAME} PUBLIC AngouriMath.CPP.Importing)
target_include_directories(${PROJECT_NAME} PUBLIC ${ANGOURIMATH_CPP_IMPORTING_PATH})

# TODO: Add tests and install targets if needed.
