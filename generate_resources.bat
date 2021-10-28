@echo off

if exist hs-card-tiles (
  rd /s /q hs-card-tiles
)

git clone -q --depth=1 https://github.com/HearthSim/hs-card-tiles.git

if not exist "Resources/Tiles" (
  mkdir "Resources/Tiles"
)
move /y "%~dp0hs-card-tiles\Tiles\*" "%~dp0Resources\Tiles" >nul

rd /s /q hs-card-tiles

msbuild ResourceGenerator\ResourceGenerator.csproj /p:Configuration=Debug /p:Platform="x86"
.\ResourceGenerator\bin\x86\Debug\ResourceGenerator.exe tiles .\Resources Tiles