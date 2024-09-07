#!/usr/bin/env sh
set -e
dotnet build
LD_LIBRARY_PATH=$LD_LIBRARY_PATH:bin/Release/net8.0/linux-x64/publish/ dotnet run
