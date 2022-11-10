#!/bin/bash
if [[ -z $1 ]]; then
    printf "Provide rid\n"
fi
dotnet publish \
-r $1 \
-c release \
-o ./publish-output \
-p:SelfContained=true \
-p:PublishAot=true \
-p:PublishTrimmed=true \
-p:TrimMode=full

# -p:PublishReadyToRun=true \
# -p:PublishSingleFile=true \
#
mv ./publish-output/CLI ./publish-output/amcli
