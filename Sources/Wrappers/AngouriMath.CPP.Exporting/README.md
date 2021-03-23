## Building exports manually

There will be archives or setup for normal use. However, if needs to be built, there are two steps:

#### 1. Generating code

First, run `generate_exports.bat` or `generate_exports.bat`. This will generate C# and C++ code which exports and 
imports functions. If you are satisfied with what is currently generated, there is no need to run this file.

#### 2. Building the library

Depending on your operating system, run `build-win-x64.bat`, `build-linux-x64.sh` or `build-mac-x64.sh`. This will generate
`dll`/`so` file as well as `pdb` and linking files into `../AngouriMath.CPP.Importing/[your-os]-x64/`.