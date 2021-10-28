Param(
    [Parameter(Mandatory=$True)]
    [int]$buildNumber,
    [Parameter(Mandatory=$True)]
    [boolean]$dev
)

$baseDir = $(Resolve-Path "$PSScriptRoot\..").Path
$projectFile = "$baseDir\Hearthstone Deck Tracker\Hearthstone Deck Tracker.csproj"

# Read version number from AssemblyInfo
$assemblyInfo = [IO.File]::ReadAllText($projectFile)
$assemblyVersionRegex = New-Object System.Text.RegularExpressions.Regex('<AssemblyVersion>(\d+)\.(\d+)\.(\d+)</AssemblyVersion>')
$match = $assemblyVersionRegex.Match($assemblyInfo)
if(!$match.Success) {
    throw "AssemblyVersion not found in csproj"
}

$major = $match.Groups[1].Value
$minor = $match.Groups[2].Value
$patch = $match.Groups[3].Value

# Construct package version
if ($dev) {
    $patch = [int]$patch + 1
}
$packageVersion = "$major.$minor.$patch"
if ($dev) {
    $packageVersion = "$packageVersion-dev$buildNumber"
}

# Update AssemblyInfo.cs with the new version
$assemblyVersion = "$major.$minor.$patch.$buildNumber"
$assemblyInfo = $assemblyVersionRegex.Replace($assemblyInfo, '<AssemblyVersion>' + $assemblyVersion + '</AssemblyVersion>')

$fileVersionRegex = New-Object System.Text.RegularExpressions.Regex('<FileVersion>.*</FileVersion>')
$assemblyInfo = $fileVersionRegex.Replace($assemblyInfo, '<FileVersion>' + $assemblyVersion + '</FileVersion>')

[IO.File]::WriteAllText($projectFile, $assemblyInfo)

Write-Host "AssemblyVersion=$assemblyVersion, PackageVersion=$packageVersion"
Write-Output $packageVersion