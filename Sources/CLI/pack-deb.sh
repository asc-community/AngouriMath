#!/bin/bash

echo 'Packing for deb'

version=$(cat ./VERSION)
name='amcli'
folder="${name}_${version}_amd64"

rm -r deb-output
mkdir deb-output
cd deb-output
mkdir "$folder"
cd "$folder"

mkdir DEBIAN
cd DEBIAN
printf "Package: $name\n" > control
printf "Version: $version\n" >> control
printf 'Architecture: amd64\n' >> control
printf 'Maintainer: WhiteBlackGoose <wbg@member.fsf.org>\n' >> control
printf 'Description: CLI Computer Algebra System based on AngouriMath\n' >> control

cd ..
mkdir -p usr/local/bin

cp ../../publish-output/amcli ./usr/local/bin/

cd ..
dpkg-deb --build --root-owner-group "$folder"
