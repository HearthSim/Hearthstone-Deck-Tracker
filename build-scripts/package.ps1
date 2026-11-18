Param(
    [Parameter(Mandatory=$True)]
    [string]$packageVersion,
    [boolean]$dev = $true
)

$baseDir = $(Resolve-Path "$PSScriptRoot\..").Path

$initialLocation = $(Get-Location).Path

$buildDir = "$baseDir\Hearthstone Deck Tracker\bin\x64"

# Run post build scripts
Set-Location "$buildDir\Velopack"
& "$PSScriptRoot\installer_post_build.bat" "Velopack-Clean"
Set-Location $PSScriptRoot

# Set up output directory
$buildsDir = if ($dev) { "dev-builds" } else { "velopack-builds" }
$output = "$baseDir\$buildsDir\$packageVersion"
if (!(Test-Path $output)) {
    mkdir $output
}

$repoUrl = if ($dev) { "https://github.com/HearthSim/HDT-dev-builds" } else { "https://github.com/HearthSim/HDT-Releases" }

# vpk is pinned in .config/dotnet-tools.json to match the Velopack package version
dotnet tool restore
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

# Download the previous full release, which vpk builds the delta against
# unauthenticated GitHub API calls share the runner IP's rate limit
$tokenArgs = if ($env:HDT_GITHUB_TOKEN) { @("--token", $env:HDT_GITHUB_TOKEN) } else { @() }
dotnet vpk download github --repoUrl $repoUrl --outputDir $output @tokenArgs
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
$previousFullPkgs = @(Get-ChildItem "$output\*-full.nupkg")

# Create velopack build
# --aumid has to match App.AppUserModelId, or shortcuts and taskbar pins stop grouping with the app.
# --signExclude limits signing to executables and HearthWatcher.dll
dotnet vpk pack `
    --packId HearthstoneDeckTracker `
    --packVersion $packageVersion `
    --packTitle "Hearthstone Deck Tracker" `
    --packAuthors HearthSim `
    --packDir "$buildDir\Velopack-Clean" `
    --mainExe HearthstoneDeckTracker.exe `
    --icon "$buildDir\Velopack\Images\HearthstoneDeckTracker.ico" `
    --framework net472 `
    --runtime win-x64 `
    --aumid com.squirrel.HearthstoneDeckTracker.HearthstoneDeckTracker `
    --signTemplate "smctl sign --simple --keypair-alias=key_1409653344 --input={{file}}" `
    --signExclude '(?i)(?<!\\HearthWatcher)\.dll$' `
    --noPortable `
    --outputDir $output
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Move-Item "$output\HearthstoneDeckTracker-win-Setup.exe" "$output\HDT-Installer.exe"

# Cleanup
$previousFullPkgs | Remove-Item

Set-Location $initialLocation
