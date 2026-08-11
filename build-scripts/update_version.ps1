Param(
    [Parameter(Mandatory=$True)]
    [int]$buildNumber,
    [Parameter(Mandatory=$True)]
    [boolean]$dev
)

$baseDir = $(Resolve-Path "$PSScriptRoot\..").Path
$projectFile = "$baseDir\Hearthstone Deck Tracker\Hearthstone Deck Tracker.csproj"

# Read version number from the csproj
$assemblyInfo = [IO.File]::ReadAllText($projectFile)
$versionRegex = New-Object System.Text.RegularExpressions.Regex('<VersionPrefix>(\d+)\.(\d+)\.(\d+)</VersionPrefix>')
$match = $versionRegex.Match($assemblyInfo)
if(!$match.Success) {
    throw "VersionPrefix not found in csproj"
}

$major = $match.Groups[1].Value
$minor = $match.Groups[2].Value
$patch = $match.Groups[3].Value

# Construct package version
if ($dev) {
    $patch = [int]$patch + 1
}
$versionPrefix = "$major.$minor.$patch"
$versionSuffix = ""
$packageVersion = $versionPrefix
if ($dev) {
    $versionSuffix = "dev$buildNumber"
    $packageVersion = "$versionPrefix-$versionSuffix"
}

# Update the csproj. AssemblyVersion derives from VersionPrefix and BuildNumber, while the Sentry
# release name derives from VersionPrefix and VersionSuffix (via InformationalVersion).
$assemblyVersion = "$versionPrefix.$buildNumber"
$assemblyInfo = $versionRegex.Replace($assemblyInfo, '<VersionPrefix>' + $versionPrefix + '</VersionPrefix>')
$assemblyInfo = [Text.RegularExpressions.Regex]::Replace($assemblyInfo, '<VersionSuffix>[^<]*</VersionSuffix>', '<VersionSuffix>' + $versionSuffix + '</VersionSuffix>')
$assemblyInfo = [Text.RegularExpressions.Regex]::Replace($assemblyInfo, '(<BuildNumber[^>]*>)\d+(</BuildNumber>)', '${1}' + $buildNumber + '${2}')

[IO.File]::WriteAllText($projectFile, $assemblyInfo)

Write-Host "AssemblyVersion=$assemblyVersion, PackageVersion=$packageVersion"
Write-Output $packageVersion