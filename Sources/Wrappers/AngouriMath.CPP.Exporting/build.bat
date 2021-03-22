echo "building for windows..."
dotnet publish /p:NativeLib=Shared /p:SelfContained=true -r win-x64 -c release
echo "built. Moving dll to the lib"
del "..\AngouriMath.CPP.Importing\AngouriMath.CPP.Exporting.dll"
echo "Deleted or did nothing. Copying the dll to the Importing"
echo F|xcopy /Y ".\bin\x64\release\netstandard2.0\win-x64\native\AngouriMath.CPP.Exporting.dll" "..\AngouriMath.CPP.Importing\AngouriMath.CPP.Exporting.dll"
echo "Moved. Moving to the playground"
echo F|xcopy /Y ".\bin\x64\release\netstandard2.0\win-x64\native\AngouriMath.CPP.Exporting.dll" "..\..\Samples\CPlusPlusPlayground\out\build\x64-Debug\AngouriMath.CPP.Exporting.dll"