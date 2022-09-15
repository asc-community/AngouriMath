#!/bin/bash
dotnet publish \
-r linux-x64 \
-c release \
-o ./publish-output/linux-x64 \
--self-contained
