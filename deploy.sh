#!/bin/sh
if [[ -z $(git status -s) ]]
then
  echo "tree is clean"
else
  echo "tree is dirty, please commit changes before running this"
  exit
fi

BASE_DIR=`pwd`
rm -rf **/bin/Release
cd MatrixRoomUtils.Web
dotnet publish -c Release
rsync -raP bin/Release/net7.0/publish/wwwroot/ rory.gay:/data/nginx/html_mru/
cd bin/Release/net7.0/publish/wwwroot
tar cf - ./ | xz -z -9 - > $BASE_DIR/MRU-BIN.tar.xz
#rsync -raP $BASE_DIR/MRU-BIN.tar.xz rory.gay:/data/nginx/html_mru/MRU-BIN.tar.xz
rm -rf $BASE_DIR/MRU-BIN.tar.xz
cd $BASE_DIR
git clone .git -b `git branch --show-current` src --recursive
rm -rf src/.git
tar cf - src/ | xz -z -9 - > MRU-SRC.tar.xz
rsync -raP $BASE_DIR/MRU-SRC.tar.xz rory.gay:/data/nginx/html_mru/MRU-SRC.tar.xz
rm -rf src/
