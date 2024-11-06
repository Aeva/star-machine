#!/usr/bin/env sh
set -e
dotnet publish StarMachine.csproj -r linux-x64 -c Release
./bin/Release/net8.0/linux-x64/publish/StarMachine $@
