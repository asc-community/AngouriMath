#!/bin/bash
cd publish-output

for arch in amd64 arm arm64
do

    if [ "$arch" = 'amd64' ]
    then
        dotnet_arch='linux-x64'
    elif [ "$arch" = 'arm' ]
    then
        dotnet_arch='linux-arm'
    else
        dotnet_arch='linux-arm64'
    fi

    version=$(cat ../../VERSION/VERSION)
    filename="angourimath-terminal_${version}_${arch}"
    
    rm -r $filename
    mkdir $filename
    cd $filename
    
    mkdir -p ./usr/local/bin/
    mkdir -p ./usr/share/applications/
    cp -r ../$dotnet_arch ./usr/local/bin
    mv ./usr/local/bin/$dotnet_arch ./usr/local/bin/angourimath-terminal-data
    echo 'cd /usr/local/bin/angourimath-terminal-data && ./AngouriMath.Terminal' > ./usr/local/bin/angourimath-terminal
    chmod +x ./usr/local/bin/angourimath-terminal
    
    mkdir DEBIAN && cd DEBIAN
    touch control
    printf "Package: angourimath-terminal\n" >> control
    printf "Version: $version\n" >> control
    printf "Architecture: $arch\n" >> control
    printf "Maintainer: WhiteBlackGoose <wbg@angouri.org>\n" >> control
    printf "Description: Terminal/CLI in F# for FOSS AngouriMath library\n" >> control
    
    cd ..
    printf "[Desktop Entry]\n" >> ./usr/share/applications/angourimath-terminal.desktop
    printf "Name=AngouriMath Terminal\n" >> ./usr/share/applications/angourimath-terminal.desktop
    printf "GenericName=Symbolic calculator and computer algebra system\n" >> ./usr/share/applications/angourimath-terminal.desktop
    printf "Comment=Perform symbolic manipulations with expressions using AngouriMath and F#\n" >> ./usr/share/applications/angourimath-terminal.desktop
    printf "Exec=x-terminal-emulator -m -e /usr/local/bin/angourimath-terminal-data/AngouriMath.Terminal\n" >> ./usr/share/applications/angourimath-terminal.desktop
    printf "Icon=/usr/local/bin/angourimath-terminal-data/icon.png\n" >> ./usr/share/applications/angourimath-terminal.desktop
    printf "Type=Application\n" >> ./usr/share/applications/angourimath-terminal.desktop
    printf "Terminal=false\n" >> ./usr/share/applications/angourimath-terminal.desktop
    printf "Categories=Office;Education;Development;\n" >> ./usr/share/applications/angourimath-terminal.desktop
    
    cp ../../icon.png ./usr/local/bin/angourimath-terminal-data/
    
    cd ..
    dpkg-deb --build --root-owner-group $filename
    mv "angourimath-terminal_${version}_${arch}.deb" "angourimath-terminal-${arch}.deb"

done
