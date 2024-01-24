#!/bin/sh
if [[ -z $(git status -s) ]]
then
  echo "tree is clean"
else
  echo "tree is dirty, please commit changes before running this"
#  exit
fi

BASE_DIR=`pwd`
rm -rf **/bin/Release
cd MatrixRoomUtils.Web
dotnet publish -c Release
rsync -raP bin/Release/net8.0/publish/wwwroot/ rory.gay:/data/nginx/html_mru/
