cd ../AngouriMath/Core/Antlr

:;# AngouriMath.Core.Antlr is the namespace of the generated lexer and parser source files
java -jar ./antlr-4.8-complete.jar -package AngouriMath.Core.Antlr ./AngouriMath.g

cd ../../../Utils

:;# Antlr's generated classes should be internal, not public
dotnet run --project Utils -c release AntlrPostProcessorReplacePublicWithInternal

:; echo Press Enter to continue...; read dummy; exit $? # Line for Unix Bash
pause REM Line for Windows Command Prompt