#!/bin/bash
dotnet publish \
-r win-x64 \
-c release \
-o ./publish-output/win-x64 \
--self-contained
