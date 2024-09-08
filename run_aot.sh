#!/usr/bin/env sh
set -e
dotnet publish -r linux-x64 -c Release
./bin/Release/net8.0/linux-x64/publish/StarMachine $@
