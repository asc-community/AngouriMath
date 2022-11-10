dotnet publish \
-r $1 \
-c release \
-o ./publish-output \
-p:PublishReadyToRun=true \
-p:SelfContained=true \
-p:PublishSingleFile=true \
-p:PublishTrimmed=true \
-p:TrimMode=link

mv ./publish-output/CLI ./publish-output/amcli
