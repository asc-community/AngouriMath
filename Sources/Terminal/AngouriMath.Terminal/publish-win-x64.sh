#!/bin/bash
dotnet publish \
-r win-x64 \
-c release \
-o ./publish-output/win-amd64 \
--self-contained
