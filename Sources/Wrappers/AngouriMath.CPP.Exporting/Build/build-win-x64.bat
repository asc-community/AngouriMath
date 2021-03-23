echo "building for 64-bit windows..."
cd ../
dotnet publish -p:NativeLib=Shared -p:SelfContained=true -r win-x64 -c release --output ../AngouriMath.CPP.Importing/win-x64/