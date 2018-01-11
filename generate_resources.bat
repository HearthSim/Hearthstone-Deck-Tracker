@echo off

msbuild /t:ResourceGenerator /p:Configuration=Debug
.\ResourceGenerator\bin\Debug\ResourceGenerator.exe .\Resources Tiles
