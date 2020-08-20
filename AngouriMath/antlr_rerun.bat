cd ./Core/Antlr
java -jar ./antlr-4.8-complete.jar -package AngouriMath.Core.Antlr ./AngouriMath.g
:;# AngouriMath.Core.Antlr is the namespace of the generated lexer and parser source files

:; echo Press Enter to continue...; read dummy; exit $? # Line for Unix Bash
pause REM Line for Windows Command Prompt