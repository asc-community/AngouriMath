cmake_minimum_required (VERSION 3.8)

project ("CPlusPlusWrapperUnitTests")

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

set(AM_TESTS_ENTRY_POINT "RunTests.cpp")



### Google Test 1/2

include(FetchContent)
FetchContent_Declare(
  googletest
  URL https://github.com/google/googletest/archive/df7fee587d442b372ef43bd66c6a2f5c9af8c5eb.zip
)
# For Windows: Prevent overriding the parent project's compiler/linker settings
set(gtest_force_shared_crt ON CACHE BOOL "" FORCE)
FetchContent_MakeAvailable(googletest)

enable_testing()

### AngouriMath 1/2

set(ANGOURIMATH_CPP_IMPORTING_PATH "../../../Wrappers/AngouriMath.CPP.Importing")
add_subdirectory(${ANGOURIMATH_CPP_IMPORTING_PATH} ${CMAKE_CURRENT_BINARY_DIR}/AngouriMath.CPP.Importing)
link_directories(./build/Debug/)

### Shared

add_executable(
  ${PROJECT_NAME}
  ${AM_TESTS_ENTRY_POINT}
)

target_link_libraries(
  ${PROJECT_NAME}
  gtest_main
)

target_link_libraries(
  ${PROJECT_NAME}
  AngouriMath.CPP.Importing
)

### GoogleTest 2/2

include(GoogleTest)
gtest_discover_tests(${PROJECT_NAME})



### AngouriMath 2/2

target_include_directories(${PROJECT_NAME} PUBLIC ${ANGOURIMATH_CPP_IMPORTING_PATH})
