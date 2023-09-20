rmdir /S /Q "..\Squirrel-Clean" 2>nul
mkdir "..\SquirrelNu"
mkdir "..\Squirrel-Clean"
mkdir "..\Squirrel-Clean/Images"
xcopy /E /Y /Q "Images\*.*" "..\Squirrel-Clean\Images"
for /D %%a in (*-*) do (
	mkdir "..\Squirrel-Clean\%%a"
	xcopy "%%a\*.*" "..\Squirrel-Clean\%%a"
)
xcopy /Y "HearthstoneDeckTracker.exe" "..\Squirrel-Clean"
xcopy /Y "HearthstoneDeckTracker.exe.config" "..\Squirrel-Clean"
xcopy /Y "*.dll" "..\Squirrel-Clean"
xcopy /Y "HearthMirror.exe" "..\Squirrel-Clean"
xcopy /Y "HearthstoneDeckTracker.pdb" "..\Squirrel-Clean"
xcopy /Y "HearthWatcher.pdb" "..\Squirrel-Clean"
