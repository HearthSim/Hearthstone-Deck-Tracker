@echo off

set SolutionDir=%~dp1
set ProjectDir=%~dp2
set TargetDir=%~dp3
set DevEnvDir=%~dp4
set OutDir=%~dp5
set SkipResourceCopy=%6

if "%SkipResourceCopy%"=="" (set SkipResourceCopy=0)

echo SolutionDir="%SolutionDir%"
echo ProjectDir="%ProjectDir%"
echo TargetDir="%TargetDir%""
echo DevEnvDir="%DevEnvDir%"
echo OutDir="%OutDir%"
echo SkipResourceCopy="%SkipResourceCopy%"

echo.
echo.

if %SkipResourceCopy% equ 1 (
  echo ** Skipping Resource Copy
  echo.
  goto EndLabel
)

:TopLabel

if exist "%SolutionDir%Resources\Generated" (
  if exist "%TargetDir%Images\Tiles" (
	echo Copying Generated tiles from "%SolutionDir%Resources\Generated" to "%TargetDir%Images\Tiles"
	xcopy /E /Y /Q "%SolutionDir%Resources\Generated" "%TargetDir%Images\Tiles"
	echo.
  )
) else (
  if exist "%SolutionDir%ResourceGenerator\%OutDir%net471\ResourceGenerator.exe" (
    "%SolutionDir%ResourceGenerator\%OutDir%net471\ResourceGenerator.exe" "%SolutionDir%Resources" Tiles 0 shell
	echo Sucessfully generated tiles in "%SolutionDir%Resources\Generated"
	goto TopLabel
  )
)

:EndLabel

echo Completed.
