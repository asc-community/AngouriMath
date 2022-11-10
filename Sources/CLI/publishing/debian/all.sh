#!/bin/bash

VERSION=$(cat ../../VERSION)
NAME='amcli'
folder="${NAME}_$(sed "s/-.*//g" $VERSION)"
printf $folder
exit
# folder="${NAME}_0.0.1"

# 1. TARBALL
echo "------------- I. Tarball"
rm -r output
mkdir output
cd output
mkdir src
cp ../../../Program.cs ./src/
cp ../../../CLI.csproj ./src/
cp ../../../Makefile ./src/
cp ../../../build.sh ./src/
cp ../../../VERSION ./src/
tar czfv ./$folder.orig.tar.gz --exclude="**/obj/**" --exclude="**/bin/**" --exclude="**/debian/**" ./src


# 2. ADD PACKAGE FILES
echo "------------- II. Package files"
export EMAIL="wbg@member.fsf.org"


cd ./src
mkdir debian
dch --create -v $VERSION --package $NAME

# compat
echo "10" > ./debian/compat

# control
printf "Source: $NAME\n" > ./debian/control
printf "Maintainer: WhiteBlackGoose <$EMAIL>\n" >> ./debian/control
printf "Section: misc\n" >> ./debian/control
printf "Priority: optional\n" >> ./debian/control
printf "Standards-Version: 3.9.2\n" >> ./debian/control
printf "Build-Depends: debhelper (>= 9)\n" >> ./debian/control
printf "\n" >> ./debian/control
printf "Package: $NAME\n" >> ./debian/control
printf "Architecture: any\n" >> ./debian/control
printf "Description: CLI Computer Algebra System based on AngouriMath" >> ./debian/control

# copyright
printf "Files: *\n" > ./debian/copyright
printf "Copyright: 2022 WhiteBlackGoose\n" >> ./debian/copyright
printf "License: CC0" >> ./debian/copyright

# rules
printf "#!/usr/bin/make -f\n" > ./debian/rules
echo '%:' >> ./debian/rules
echo '	dh $@' >> ./debian/rules
printf "\n" >> ./debian/rules
printf "override_dh_auto_install:\n" >> ./debian/rules
echo "	\$(MAKE) DESTDIR=\$\$(pwd)/debian/$NAME prefix=/usr install" >> ./debian/rules

# hello-world.dirs
echo 'usr/bin' > ./debian/$NAME.dirs

# source/format
mkdir ./debian/source
printf "3.0 (quilt)" > ./debian/source/format


# 3. BUILD PACKAGE
echo "------------ III. Build "
debuild
