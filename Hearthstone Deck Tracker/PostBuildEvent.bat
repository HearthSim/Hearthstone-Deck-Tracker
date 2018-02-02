@echo off

set SolutionDir=%~dp1
set ProjectDir=%~dp2
set TargetDir=%~dp3
set DevEnvDir=%~dp4
set OutDir=%5
set SkipResourceCopy=%6
set MsBuild=%7
set SDKTools=C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\amd64\

if "%SkipResourceCopy%"=="" (set SkipResourceCopy=0)

echo SolutionDir="%SolutionDir%"
echo ProjectDir="%ProjectDir%"
echo TargetDir="%TargetDir%"
echo DevEnvDir="%DevEnvDir%"
echo OutDir="%OutDir%"
echo SkipResourceCopy="%SkipResourceCopy%"
echo MsBuild="%MsBuild%"

echo.
echo.

if %SkipResourceCopy% equ 1 (
  echo ** Skipping Resource Copy
  echo.
  goto EndLabel
)

:TopLabel

if exist "%SolutionDir%ResourceGenerator\%OutDir%ResourceGenerator.exe" (
  echo Running Resource Generator
  start /WAIT /D "%SolutionDir%ResourceGenerator\%OutDir%" ResourceGenerator.exe "%SolutionDir%Resources" Tiles 0 %MsBuild%
  echo Sucessfully generated tiles in "%SolutionDir%Resources\Generated"
  echo.
  echo Copying Generated tiles from "%SolutionDir%Resources\Generated" to "%TargetDir%Images\Tiles"
  xcopy /E /Y /Q "%SolutionDir%Resources\Generated" "%TargetDir%Images\Tiles"
  echo.
) else (
  echo  "%SolutionDir%ResourceGenerator\%OutDir%ResourceGenerator.exe"
  echo Enable building the ResourceGenerator project from Build -^> Configuration Manager...
  echo.
  exit /B 1
)


:EndLabel

echo Completed.
