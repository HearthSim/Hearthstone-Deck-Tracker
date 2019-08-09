@echo off

msbuild /t:ResourceGenerator /p:Configuration=Debug /p:Platform="x86"
.\ResourceGenerator\bin\x86\Debug\ResourceGenerator.exe whizbang whizbang.json