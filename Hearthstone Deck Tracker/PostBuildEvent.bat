@echo off

set SolutionDir=%~dp1
set ProjectDir=%~dp2
set TargetDir=%~dp3
set DevEnvDir=%~dp4
set SkipResourceCopy=%5

if "%SkipResourceCopy%"=="" (set SkipResourceCopy=0)

echo SolutionDir="%SolutionDir%"
echo ProjectDir="%ProjectDir%"
echo TargetDir="%TargetDir%""
echo DevEnvDir="%DevEnvDir%"
echo SkipResourceCopy="%SkipResourceCopy%"

echo.
echo.

if %SkipResourceCopy% equ 1 (
  echo ** Skipping Resource Copy
  echo.
  goto EndLabel
)

:TopLabel

if exist "%SolutionDir%Resources\Generated\Tiles" (
  if exist "%TargetDir%Images\Tiles" (
	echo Copying Generated tiles from "%SolutionDir%Resources\Generated\Tiles" to "%TargetDir%Images\Tiles"
	xcopy /E /Y /Q "%SolutionDir%Resources\Generated\Tiles" "%TargetDir%Images\Tiles"
	echo.
  )
) else (
  if exist "%SolutionDir%ResourceGenerator\bin\Debug\net471\ResourceGenerator.exe" (
    "%SolutionDir%ResourceGenerator\bin\Debug\net471\ResourceGenerator.exe" "%SolutionDir%Resources" Tiles 0 msbuild
	echo Sucessfully generated tiles at "%SolutionDir%Resources\Generated\Tiles"
	goto TopLabel
  )
)

:EndLabel

echo Completed.
