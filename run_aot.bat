
call build_shaders.bat
dotnet publish -r win-x64 -c Release
copy third_party\sdl3\Debug\SDL3.dll bin\Release\net8.0\win-x64\publish\
.\bin\Release\net8.0\win-x64\publish\StarMachine
