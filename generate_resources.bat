@echo off

msbuild /t:ResourceGenerator /p:Configuration=Debug
.\ResourceGenerator\bin\x86\Debug\ResourceGenerator.exe .\Resources Tiles