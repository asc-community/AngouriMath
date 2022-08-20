#!/bin/bash

echo 'Packing for deb'

version='0.1-alpha'
name='angourimath-cli'
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
printf 'Maintainer: WhiteBlackGoose <wbg@angouri.org>\n' >> control
printf 'Description: terminal for AngouriMath.CLI\n' >> control

cd ..
mkdir -p usr/local/bin
cp ../../start-cli.sh ./usr/local/bin/angourimath-cli
chmod +x ./usr/local/bin/angourimath-cli

cp -r ../../../native-out ./usr/local/bin/amcli-data

cd ..
dpkg-deb --build --root-owner-group "$folder"
