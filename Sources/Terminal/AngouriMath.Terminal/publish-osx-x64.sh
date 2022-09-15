#!/bin/bash
dotnet publish \
-r osx-x64 \
-c release \
-o ./publish-output/osx-x64 \
--self-contained
