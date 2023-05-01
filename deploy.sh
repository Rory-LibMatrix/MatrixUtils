#!/bin/sh
cd MatrixRoomUtils.Web
dotnet publish -c Release
rsync -raP bin/Release/net7.0/publish/wwwroot/ rory.gay:/data/nginx/html_mru/
