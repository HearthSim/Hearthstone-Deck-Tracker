[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

if (Test-Path "nuget.exe") {
	Invoke-Expression "./nuget restore"
}
elseif((Get-Command "nuget" -ErrorAction SilentlyContinue) -ne $null) {
	nuget restore
}
else {
	"Nuget was not found and is required to run bootstrap.ps. Download and retry now?"
	choice /c yn
	if ($LASTEXITCODE -eq 1) {
		"Downloading nuget..."
		$nugetLocation = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
		(New-Object Net.WebClient).DownloadFile($nugetLocation, "$PSScriptRoot\nuget.exe")
		Invoke-Expression "./nuget restore"
	}
}

try {
	git --version
}
catch {
	"Git was not found and is required to run bootstrap.bat. Download git from https://git-scm.com/download and during installation choose `"Use Git from the Windows Command Prompt`"."
}

function FetchLib($name) {
	"Fetching $name..."
	$url = "https://libs.hearthsim.net/hdt/$name.dll"
	(New-Object Net.WebClient).DownloadFile($url, "$PSScriptRoot\lib\$name.dll")
}

FetchLib "HearthDb"
FetchLib "HearthMirror"
FetchLib "HSReplay"
FetchLib "BobsBuddy"

git clone -q "https://github.com/HearthSim/HDT-Localization" "HDT-Localization"

Copy-Item "HDT-Localization\*.resx" "Hearthstone Deck Tracker\Properties\" -Force

Invoke-Expression "./generate_resources.bat"
