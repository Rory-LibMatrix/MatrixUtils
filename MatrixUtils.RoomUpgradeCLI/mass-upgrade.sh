#! /usr/bin/env sh
dotnet build -c Release
cat lst | while read id
do
  DOTNET_ENVIRONMENT=Local dotnet bin/Release/net9.0/MatrixUtils.RoomUpgradeCLI.dll new tmp/$id.json --upgrade $id --upgrade-unstable-values --force-upgrade --invite-powerlevel-users \; \
  import-upgrade-state tmp/$id.json \; \
  modify tmp/$id.json --version 12 &
done
wait