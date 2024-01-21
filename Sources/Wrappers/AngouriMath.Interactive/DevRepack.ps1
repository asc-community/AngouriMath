# This file is for developers only

# clean up the previously-cached NuGet packages
Remove-Item -Recurse ~\.nuget\packages\AngouriMath.Interactive* -Force
Remove-Item -Recurse ~\.nuget\packages\AngouriMath.FSharp* -Force

# build AngouriMath.Interactive
dotnet restore
dotnet clean
dotnet build -c Release /p:Version=10.0.0
dotnet pack -c Release /p:Version=10.0.0

Write-Host "Path to package: $PSScriptRoot/bin/Release"
