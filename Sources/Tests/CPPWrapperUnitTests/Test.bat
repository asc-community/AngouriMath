cd tests
cmake -S . -B build
mkdir "build\Debug"
copy "..\..\..\Wrappers\AngouriMath.CPP.Importing\win-x64\AngouriMath.CPP.Exporting.dll" "build\Debug\AngouriMath.CPP.Exporting.dll"
cmake --build build
cd build
ctest
cd ../..
pause