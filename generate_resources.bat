@echo off

call update_card_tiles.bat

msbuild ResourceGenerator\ResourceGenerator.csproj /p:Configuration=Debug /p:Platform="x86"
.\ResourceGenerator\bin\x86\Debug\ResourceGenerator.exe tiles .\Resources Tiles