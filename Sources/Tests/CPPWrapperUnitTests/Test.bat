cd tests
cmake -S . -B build
cmake --build build
cd build
ctest
cd ../..
pause