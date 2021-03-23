echo "building for 64-bit linux..."
cd ../
dotnet publish -p:NativeLib=Shared -p:SelfContained=true -r linux-x64 -c release --output ../AngouriMath.CPP.Importing/linux-x64/