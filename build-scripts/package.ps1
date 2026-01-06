Param(
    [Parameter(Mandatory=$True)]
    [string]$packageVersion,
    [boolean]$dev = $true
)

$baseDir = $(Resolve-Path "$PSScriptRoot\..").Path

$initialLocation = $(Get-Location).Path

$DEV_LATEST = "https://api.github.com/repos/HearthSim/HDT-dev-builds/releases/latest"
$PROD_LATEST = "https://api.github.com/repos/HearthSim/HDT-Releases/releases/latest"

$buildDir = "$baseDir\Hearthstone Deck Tracker\bin\x86"
$hdtReleaseDir = "$buildDir\Hearthstone Deck Tracker"
$squirrelTools = "$baseDir\packages\squirrel.windows\1.9.1\tools"
$squirrel = "$squirrelTools\Squirrel.exe"

[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

# Run post build scripts
if (!$dev) {
    Set-Location "$buildDir\Release"
    .$PSScriptRoot/release_post_build.bat
    Set-Location $PSScriptRoot
}
Set-Location "$buildDir/Squirrel"
.$PSScriptRoot/squirrel_post_build.bat
Set-Location $PSScriptRoot

# Set up output directory
$buildsDir = if ($dev) { "dev-builds" } else { "builds" };
$output = "$baseDir\$buildsDir\$packageVersion"
if (!(Test-Path $output)) {
    mkdir $output
}

# Sign and zip up portable build
if (!$dev) {
    smctl sign --simple --keypair-alias=key_1409653344 --input="$hdtReleaseDir\HDTUpdate.exe"
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE  }

    smctl sign --simple --keypair-alias=key_1409653344 --input="$hdtReleaseDir\HDTUninstaller.exe"
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE  }

    smctl sign --simple --keypair-alias=key_1409653344 --input="$hdtReleaseDir\Hearthstone Deck Tracker.exe"
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE  }

    Set-Location $buildDir
    7z a -r -mx9 "$output\Hearthstone.Deck.Tracker-v$packageVersion.zip" "Hearthstone Deck Tracker"
}

# Create squirrel build
$url = if ($dev) { $DEV_LATEST } else { $PROD_LATEST }
$json = (Invoke-WebRequest $url -UseBasicParsing).Content | ConvertFrom-Json
$wc = New-Object System.Net.WebClient
$oldFullPkg = $null
foreach ($asset in $json.assets) {
	if ($asset.name -eq "HDT-Installer.exe") {
		continue
	}
	"Downloading $($asset.name)..."
    $file = "$output\$($asset.name)"
	$wc.DownloadFile($asset.browser_download_url, $file)
    if ($asset.name.endswith("-full.nupkg")) {
        $oldFullPkg = $file
    }
}

# Set up signtool shim. Squirrel uses signtool internally, but we need to call smctl.
"Building SignToolShim..."
msbuild "$baseDir\SignToolShim\SignToolShim.csproj" /p:Configuration=Release /v:minimal
Copy-Item "$baseDir\SignToolShim\bin\Release\SignToolShim.exe" "$squirrelTools\signtool.exe" -Force -Verbose

nuget pack "$PSScriptRoot\hdt.nuspec" -Version $packageVersion -Properties Configuration=Release -OutputDirectory "$buildDir\SquirrelNu"
$icon = "$buildDir\Squirrel\Images\HearthstoneDeckTracker.ico"
$nupkg = "$buildDir\SquirrelNu\HearthstoneDeckTracker.$packageVersion.nupkg"
& $squirrel --releasify $nupkg --releaseDir=$output --setupIcon=$icon --icon=$icon --no-msi -n "shim" --framework-version=net472 | Out-Default
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE  }

Move-Item "$output\Setup.exe" "$output\HDT-Installer.exe"

# Cleanup
Remove-Item $oldFullPkg
Set-Location $initialLocation