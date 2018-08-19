@echo off

call update_card_tiles.bat

msbuild /t:ResourceGenerator /p:Configuration=Debug /p:Platform="x86"
.\ResourceGenerator\bin\x86\Debug\ResourceGenerator.exe .\Resources Tiles