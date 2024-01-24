#!/bin/sh

ssh 192.168.0.13 "bash -xc 'sudo rm -rf /tmp/mru-build'"
ssh 192.168.0.13 "bash -xc 'sudo mkdir /tmp/mru-build'"
ssh 192.168.0.13 "bash -xc 'sudo chown \`basename \$PWD\`: /tmp/mru-build'"
rsync -raP ./ 192.168.0.13:/tmp/mru-build
ssh 192.168.0.13 "sh -c 'cd /tmp/mru-build/MatrixRoomUtils.Web; dotnet clean --r -v:n'"
#ssh 192.168.0.13 "sh -c 'cd /tmp/mru-build/MatrixRoomUtils.Web; dotnet build -c Release'"
ssh 192.168.0.13 "sh -c 'cd /tmp/mru-build/MatrixRoomUtils.Web; dotnet publish -c Release'"
rsync -raP 192.168.0.13:/tmp/mru-build/MatrixRoomUtils.Web/bin/Release/net8.0/publish/wwwroot/ /tmp/mru-wwwroot --delete
rsync -raP /tmp/mru-wwwroot/ rory.gay:/data/nginx/html_mru --delete
ssh rory.gay chmod o+r /data/nginx/html_mru -Rc
ssh rory.gay sudo find /data/nginx/html_mru -type d -exec chmod o+rx {} +

echo "-- End of script! --"
