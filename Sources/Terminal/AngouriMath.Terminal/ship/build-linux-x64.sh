cd ..
dotnet publish \
-r linux-x64 \
-c release \
-o ./native-out \
-p:SelfContained=true
