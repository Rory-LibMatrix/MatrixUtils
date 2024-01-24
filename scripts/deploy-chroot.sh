#!/bin/sh

#  if [ -f "/tmp/mru-build/dev/null" ]; then sudo umount /tmp/mru-build/dev || exit 1; fi
#  if [ -f "/tmp/mru-build/proc/uptime" ]; then sudo umount /tmp/mru-build/proc || exit 1; fi
#  if [ -d "/tmp/mru-build/sys/power" ]; then sudo umount /tmp/mru-build/sys || exit 1; fi
#  sudo rm -rf /tmp/mru-build
#  mkdir /tmp/mru-build
#  
#  # ARCH
#  #sudo pacstrap -C ./pacman.conf -c -G -M -P /tmp/mru-build  dotnet-sdk aspnet-runtime busybox kitty-terminfo
#  #sudo arch-chroot /tmp/mru-build sh -c 'for i in `busybox --list-full`; do busybox ln /bin/busybox /$i; done'
#  
#  # DEBIAN 
#  sudo debootstrap stable /tmp/mru-build http://deb.debian.org/debian
#  sudo arch-chroot /tmp/mru-build bash --login -c 'apt update; apt upgrade -y; apt install -y curl'
#  sudo arch-chroot /tmp/mru-build bash --login -c 'curl https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb; echo $PATH; dpkg -i packages-microsoft-prod.deb; rm packages-microsoft-prod.deb'
#  sudo arch-chroot /tmp/mru-build bash --login -c 'apt update; apt upgrade -y; apt install -y dotnet-sdk-8.0'


#git clone --recursive .git /tmp/mru-build/tmp
sudo rm -rf /tmp/mru-build/build/
cp ./ /tmp/mru-build/build/ -r
#sudo arch-chroot /tmp/mru-build bash --login -c 'ping nuget.org -c 4'
sudo arch-chroot /tmp/mru-build bash --login -c 'cd /build; dotnet restore -v:n'
sudo arch-chroot /tmp/mru-build bash --login -c 'cd /build; dotnet clean --r -v:n'
#sudo arch-chroot /tmp/mru-build sh -c 'cd /build; dotnet build -v d'

echo "-- End of script! --"