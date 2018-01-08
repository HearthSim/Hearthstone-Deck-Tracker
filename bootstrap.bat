@echo off

nuget restore
git clone https://github.com/HearthSim/HearthDb HearthDb
git clone https://github.com/HearthSim/HearthMirror HearthMirror
git clone https://github.com/HearthSim/HSReplay-API-Client.git HSReplay-Api
git clone https://github.com/HearthSim/HDT-Localization HDT-Localization
xcopy /Y "HDT-Localization\*.resx" "Hearthstone Deck Tracker\Properties\"
./generate_resources.bat