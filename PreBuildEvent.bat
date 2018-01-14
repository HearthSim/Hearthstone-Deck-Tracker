@echo off

set SolutionDir=%~dp1
set ProjectDir=%~dp2
set TargetDir=%~dp3
set DevEnvDir=%~dp4
set SkipGitSync=%5
set SkipResourceCopy=%6

if "%SkipGitSync%"=="" (set SkipGitSync=0)
if "%SkipResourceCopy%"=="" (set SkipResourceCopy=0)

echo SolutionDir="%SolutionDir%"
echo ProjectDir="%ProjectDir%"
echo TargetDir="%TargetDir%""
echo DevEnvDir="%DevEnvDir%"
echo SkipGitSync="%SkipGitSync%"
echo SkipResourceCopy="%SkipResourceCopy%"

echo.
echo.

if %SkipGitSync% equ 1 if %SkipResourceCopy% equ 1 (
	echo ** Skipping GitSync and Resource Copy
	echo.
	goto EndLabel
)

if %SkipGitSync% equ 1 (
	echo ** Skipping GitSync
	echo.
	goto ResourceCopyLabel
)

if exist "%SolutionDir%HearthDb" (
  echo Updating "%SolutionDir%HearthDb" to origin/master
  git -C "%SolutionDir%HearthDb" fetch
  git -C "%SolutionDir%HearthDb" reset --hard origin/master
) else (
  git clone --depth 1 https://github.com/HearthSim/HearthDb.git "%SolutionDir%HearthDb"
)

echo.

if exist "%SolutionDir%HearthMirror" (
  echo Updating "%SolutionDir%HearthMirror" to origin/master
  git -C "%SolutionDir%HearthMirror" fetch
  git -C "%SolutionDir%HearthMirror" reset --hard origin/master
) else (
  git clone --depth 1 https://github.com/HearthSim/HearthMirror.git "%SolutionDir%HearthMirror"
)

echo.

if exist "%SolutionDir%HSReplay-Api" (
  echo Updating "%SolutionDir%HSReplay"-Api to origin/master
  git -C "%SolutionDir%HSReplay-Api" fetch
  git -C "%SolutionDir%HSReplay-Api" reset --hard origin/master
) else (
  git clone --depth 1 https://github.com/HearthSim/HSReplay-API-Client.git "%SolutionDir%HSReplay-Api"
)

echo.

if exist "%SolutionDir%HDT-Localization" (
  echo Updating "%SolutionDir%HDT-Localization" to origin/master
  git -C "%SolutionDir%HDT-Localization" fetch
  git -C "%SolutionDir%HDT-Localization" reset --hard origin/master
) else (
  git clone --depth 1 https://github.com/HearthSim/HDT-Localization.git "%SolutionDir%HDT-Localization"
)

echo.

:ResourceCopyLabel

if %SkipResourceCopy% equ 1 (
  echo ** Skipping Resource Copy
  echo.
  goto EndLabel
)

if exist "%SolutionDir%HDT-Localization" if exist "%ProjectDir%Properties\" (
	echo Copying Localization files from "%SolutionDir%HDT-Localization" to "%ProjectDir%Properties\"
	xcopy /Y "%SolutionDir%HDT-Localization\*.resx" "%ProjectDir%Properties\"
	echo.
)

if not exist "%TargetDir%Images\Tiles" (
	echo Creating missing directory "%TargetDir%Images\Tiles"
	mkdir "%TargetDir%Images\Tiles"
	echo.
)

if not exist "%TargetDir%Images\Themes" (
	echo Creating missing directory "%TargetDir%Images\Themes"
	mkdir "%TargetDir%Images\Themes"
	echo.
)

if exist "%ProjectDir%Images\Themes" if exist "%TargetDir%Images\Themes" (
	echo Copying Themes from "%ProjectDir%Images\Themes" to "%TargetDir%Images\Themes"
	xcopy /E /Y /Q "%ProjectDir%Images\Themes" "%TargetDir%Images\Themes"
	echo.
)

if exist "%SolutionDir%CHANGELOG.md" if exist "%ProjectDir%Resources" (
	echo Copying "%SolutionDir%CHANGELOG.md" to "%ProjectDir%Resources"
	xcopy /Y /Q "%SolutionDir%CHANGELOG.md" "%ProjectDir%Resources"
)

:EndLabel

echo Completed.
