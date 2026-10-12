Param(
    [Parameter(Mandatory=$True)]
    [string]$packageVersion,
    [boolean]$dev = $true,
    [ValidateSet("squirrel", "velopack")]
    [string]$updater = "velopack"
)

if ($dev -and $updater -eq "squirrel") {
    throw "Dev builds are always packaged with Velopack"
}

$baseDir = $(Resolve-Path "$PSScriptRoot\..").Path

$initialLocation = $(Get-Location).Path

$buildDir = "$baseDir\Hearthstone Deck Tracker\bin\x64"
$configuration = if ($updater -eq "squirrel") { "Squirrel" } else { "Velopack" }

# Run post build scripts
Set-Location "$buildDir\$configuration"
& "$PSScriptRoot\installer_post_build.bat" "$configuration-Clean"
Set-Location $PSScriptRoot

# Set up output directory
$buildsDir = if ($dev) { "dev-builds" } elseif ($updater -eq "squirrel") { "builds" } else { "velopack-builds" }
$output = "$baseDir\$buildsDir\$packageVersion"
if (!(Test-Path $output)) {
    mkdir $output
}

if ($updater -eq "squirrel") {
    $squirrelTools = "$baseDir\packages\squirrel.windows\1.9.1\tools"
    $squirrel = "$squirrelTools\Squirrel.exe"

    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

    # Create squirrel build
    $json = (Invoke-WebRequest "https://api.github.com/repos/HearthSim/HDT-Releases/releases/latest" -UseBasicParsing).Content | ConvertFrom-Json
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

    $csproj = [xml](Get-Content "$baseDir\Hearthstone Deck Tracker\Hearthstone Deck Tracker.csproj")
    $copyright = @($csproj.Project.PropertyGroup.Copyright | Where-Object { $_ })[0]
    New-Item -ItemType Directory -Force "$buildDir\SquirrelNu" | Out-Null
    nuget pack "$PSScriptRoot\hdt.nuspec" -Version $packageVersion -Properties "Configuration=Release;copyright=$copyright" -OutputDirectory "$buildDir\SquirrelNu"
    $icon = "$buildDir\Squirrel\Images\HearthstoneDeckTracker.ico"
    $nupkg = "$buildDir\SquirrelNu\HearthstoneDeckTracker.$packageVersion.nupkg"
    & $squirrel --releasify $nupkg --releaseDir=$output --setupIcon=$icon --icon=$icon --no-msi -n "shim" --framework-version=net472 | Out-Default
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE  }

    Move-Item "$output\Setup.exe" "$output\HDT-Installer.exe"

    # Cleanup
    Remove-Item $oldFullPkg
}
else {
    $repoUrl = if ($dev) { "https://github.com/HearthSim/HDT-dev-builds" } else { "https://github.com/HearthSim/HDT-Velopack-Releases" }

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
    # --signExclude keeps the shim's selection of signing only executables and HearthWatcher.dll.
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
}

Set-Location $initialLocation
