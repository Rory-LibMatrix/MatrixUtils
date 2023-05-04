#!/bin/sh
BASE_DIR=`pwd`
rm -rf **/bin/Release
cd MatrixRoomUtils.Web
dotnet publish -c Release
rsync -raP bin/Release/net7.0/publish/wwwroot/ rory.gay:/data/nginx/html_mru/
cd bin/Release/net7.0/publish/wwwroot
tar cf - ./ | xz -z -9 - > $BASE_DIR/MRU.tar.xz
rsync -raP $BASE_DIR/MRU.tar.xz rory.gay:/data/nginx/html_mru/MRU.tar.xz
rm -rf $BASE_DIR/MRU.tar.xz
