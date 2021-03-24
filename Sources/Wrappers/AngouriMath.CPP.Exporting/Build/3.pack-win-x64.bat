cd ../../
tar --exclude=./AngouriMath.CPP.Importing/out --exclude=./AngouriMath.CPP.Importing/.vs -c -f AngouriMath-Win-x64.zip ./AngouriMath.CPP.Importing
tar --exclude=./AngouriMath.CPP.Importing/out --exclude=./AngouriMath.CPP.Importing/.vs --exclude=./AngouriMath.CPP.Importing/win-x64/AngouriMath.CPP.Exporting.pdb -c -f AngouriMath-Win-x64-No-Pdb.zip ./AngouriMath.CPP.Importing