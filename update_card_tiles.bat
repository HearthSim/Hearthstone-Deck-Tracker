if exist hs-card-tiles (
  rd /s /q hs-card-tiles
)

git clone --depth=1 https://github.com/HearthSim/hs-card-tiles.git

if not exist "Resources/Tiles" (
  mkdir "Resources/Tiles"
)
move /y "%~dp0hs-card-tiles\Tiles\*" "%~dp0Resources\Tiles" >nul

rd /s /q hs-card-tiles