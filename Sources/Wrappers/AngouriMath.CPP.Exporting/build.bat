echo "building for windows..."
dotnet publish -p:NativeLib=Shared -p:SelfContained=true -r win-x64 -c release --output ../AngouriMath.CPP.Importing/