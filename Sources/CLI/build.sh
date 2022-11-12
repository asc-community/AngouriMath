#!/bin/bash
if [[ -z $1 ]]; then
    printf "Provide rid\n"
    exit
fi

rm -r bin
rm -r obj

/home/goose/programs/dotnet-folder/dotnet publish \
-r $1 \
-c release \
-o ./publish-output \
-p:SelfContained=true \
-p:PublishAot=true \
-p:PublishTrimmed=true \
-p:TrimMode=full \
-p:IlcInvariantGlobalization=true \
-p:IlcOptimizationPreference=Speed \
-p:IlcDisableReflection=false \
-p:StripSymbols=true \
-p:Version=$(cat ./VERSION/VERSION)

# -p:IlcScanReflection=false \
mv ./publish-output/CLI ./publish-output/amcli
