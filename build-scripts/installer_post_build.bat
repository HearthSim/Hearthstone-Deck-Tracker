set outDir=..\%~1
rmdir /S /Q "%outDir%" 2>nul
mkdir "%outDir%"
mkdir "%outDir%\Images"
xcopy /E /Y /Q "Images\*.*" "%outDir%\Images"
for /D %%a in (*-*) do (
	mkdir "%outDir%\%%a"
	xcopy "%%a\*.*" "%outDir%\%%a"
)
xcopy /Y "HearthstoneDeckTracker.exe" "%outDir%"
xcopy /Y "HearthstoneDeckTracker.exe.config" "%outDir%"
xcopy /Y "*.dll" "%outDir%"
xcopy /Y "HearthMirror.dll" "%outDir%"
