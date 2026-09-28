#!/bin/bash


# Portable runtime identifiers: since .NET 8 a versioned one such as osx.12-arm64 is not
# recognised, and the publish fails with NETSDK1083.
for arch in linux-x64 linux-arm linux-arm64 win-x64 win-x86 osx-x64 osx-arm64
do
    dotnet publish \
    -r $arch \
    -c Release \
    -o ./publish-output/$arch \
    --self-contained
done

