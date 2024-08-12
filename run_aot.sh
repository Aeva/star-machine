#!/usr/bin/env sh
sh build_shaders.sh
dotnet publish -r linux-x64 -c Release
cp ../sdl3gpu/SDL/build/libSDL3.so bin/Release/net8.0/linux-x64/publish/
cp ../plutovg/build/libplutovg.so bin/Release/net8.0/linux-x64/publish/
cp ../plutosvg/build/libplutosvg.so bin/Release/net8.0/linux-x64/publish/
./bin/Release/net8.0/linux-x64/publish/StarMachine
