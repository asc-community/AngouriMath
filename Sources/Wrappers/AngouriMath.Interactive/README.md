### Docs for contributors

To test the extension in notebooks
1. Run `DevRepack.ps1` in powershell
2. Copy the produced path in the end
3. Run these
```
#i "the_path_from_script"
#r "nuget:AngouriMath.Interactive, *-*"
```
replacing `the_path_from_script` with the path you just copied.