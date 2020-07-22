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
$squirrelTools = "$baseDir\packages\squirrel.windows.1.9.1\tools"
$signtool = "$squirrelTools\signtool.exe"
$squirrel = "$squirrelTools\Squirrel.exe"
$cert = "$baseDir\cert.pfx"

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

# Generate cert from environment
if (!(Test-Path "$cert")) {
    [IO.File]::WriteAllBytes("$cert", [Convert]::FromBase64String($Env:CERT))
}

# Sign and zip up portable build
if (!$dev) {
    & $signtool sign /tr "http://timestamp.digicert.com" /a /f $cert /p $Env:CERT_PASSWORD "$hdtReleaseDir\HDTUpdate.exe" "$hdtReleaseDir\HDTUninstaller.exe" "$hdtReleaseDir\Hearthstone Deck Tracker.exe" | Out-Default
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
nuget pack "$PSScriptRoot\hdt.nuspec" -Version $packageVersion -Properties Configuration=Release -OutputDirectory "$buildDir\SquirrelNu"
$icon = "$buildDir\Squirrel\Images\HearthstoneDeckTracker.ico"
$nupkg = "$buildDir\SquirrelNu\HearthstoneDeckTracker.$packageVersion.nupkg"
$certInfo = "/tr http://timestamp.digicert.com /a /f $cert /p $Env:CERT_PASSWORD"
& $squirrel --releasify $nupkg --releaseDir=$output --setupIcon=$icon --icon=$icon --no-msi -n $certInfo --framework-version=net472 | Out-Default
& $signtool sign /tr "http://timestamp.digicert.com" /a /f $cert /p $Env:CERT_PASSWORD "$output\Setup.exe" | Out-Default
Move-Item "$output\Setup.exe" "$output\HDT-Installer.exe"

# Cleanup
Remove-Item $oldFullPkg
Set-Location $initialLocation