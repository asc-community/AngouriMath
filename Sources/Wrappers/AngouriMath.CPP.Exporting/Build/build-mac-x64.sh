echo "building for 64-bit mac..."
cd ../
dotnet publish -p:NativeLib=Shared -p:SelfContained=true -r mac-x64 -c release --output ../AngouriMath.CPP.Importing/mac-x64/