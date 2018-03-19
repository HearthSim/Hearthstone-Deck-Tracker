@echo off
set nuget_location=https://dist.nuget.org/win-x86-commandline/latest/nuget.exe

:startup
nuget restore || goto :nugetmissing
git clone https://github.com/HearthSim/HearthDb HearthDb || goto :gitmissing
git clone https://github.com/HearthSim/HearthMirror HearthMirror
git clone https://github.com/HearthSim/HSReplay-API-Client.git HSReplay-Api
git clone https://github.com/HearthSim/HDT-Localization HDT-Localization
xcopy /Y "HDT-Localization\*.resx" "Hearthstone Deck Tracker\Properties\"
./generate_resources.bat
goto :EOF

:nugetmissing
echo Nuget was not found and is required to run bootstrap.bat. Download and retry now?
choice /c yn
if %ERRORLEVEL%==2 goto :EOF
powershell -Command "(New-Object Net.WebClient).DownloadFile('%nuget_location%', 'nuget.exe')"
goto :startup

:gitmissing
echo Git was not found and is required to run bootstrap.bat. Download git from https://git-scm.com/download and during installation choose "Use Git from the Windows Command Prompt".
pause
