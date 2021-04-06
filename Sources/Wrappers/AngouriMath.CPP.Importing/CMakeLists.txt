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
"AngouriMath.cpp"
"ErrorCode.cpp")

add_library(${PROJECT_NAME} ${SOURCES})

# link_directories(./out-x64/)
target_link_directories(${PROJECT_NAME} PUBLIC out-x64)
if (WIN32)
	target_link_libraries(${PROJECT_NAME} PUBLIC AngouriMath.CPP.Exporting)
else ()
	target_link_libraries(${PROJECT_NAME} PUBLIC -lAngouriMath.CPP.Exporting)
endif()
