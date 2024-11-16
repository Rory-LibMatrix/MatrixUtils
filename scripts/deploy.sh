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
cd MatrixUtils.Web
dotnet publish -c Release
rsync --delete -raP bin/Release/net9.0/publish/wwwroot/ rory.gay:/data/nginx/html_mru/
