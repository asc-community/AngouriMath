#!/bin/bash


for arch in linux-x64 linux-arm linux-arm64 win-x64 win-x86 osx-x64 osx.12-arm64
do
    dotnet publish \
    -r $arch \
    -c release \
    -o ./publish-output/$arch \
    --self-contained
done

