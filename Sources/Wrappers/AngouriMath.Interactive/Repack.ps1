# clean up the previously-cached NuGet packages
Remove-Item -Recurse ~\.nuget\packages\AngouriMath.Interactive* -Force
Remove-Item -Recurse ~\.nuget\packages\AngouriMath.FSharp* -Force

# build AngouriMath.Interactive
dotnet restore
dotnet clean
dotnet build -c Release
dotnet pack -c Release