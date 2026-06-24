@echo off

msbuild ResourceGenerator\ResourceGenerator.csproj /p:Configuration=Debug /p:Platform="x64"
.\ResourceGenerator\bin\x64\Debug\ResourceGenerator.exe whizbang whizbang.json