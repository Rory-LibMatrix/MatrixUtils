# Rory&::MatrixUtils

Power tools for Matrix.

# Installation

```sh
git clone --recursive $REPO
cd MatrixUtils/MatrixUtils.Web
dotnet publish
cp -rv bin/Release/net8.0/publish/wwwroot /var/www/html_rmu
```

# Contributing

```sh
# Prepare patch set
mkdir patches
git format-patch --output-directory "./patches" @{u}..

# Send patches
```

### Developer utility commands

Error reporting upon file save:
```sh
inotifywait -rmqe CLOSE_WRITE --include '.*\.cs$' . | while read l; do clear; dotnet build --property WarningLevel=0; done
```

Hot rebuild on file save:
```sh
dotnet watch run --no-hot-reload --property WarningLevel=0
```
