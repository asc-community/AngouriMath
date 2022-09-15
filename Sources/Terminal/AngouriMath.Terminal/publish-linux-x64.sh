#!/bin/bash
dotnet publish \
-r linux-x64 \
-c release \
-o ./publish-output/linux-amd64 \
--self-contained
