#!/bin/bash
dotnet publish \
-r osx-x64 \
-c release \
-o ./publish-output/osx-amd64 \
--self-contained
