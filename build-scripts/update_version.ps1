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
$versionRegex = New-Object System.Text.RegularExpressions.Regex('<Version>(\d+)\.(\d+)\.(\d+)[^<]*</Version>')
$match = $versionRegex.Match($assemblyInfo)
if(!$match.Success) {
    throw "Version not found in csproj"
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

# Update the csproj. AssemblyVersion derives from Version (minus any suffix) and BuildNumber, while
# the Sentry release name derives from the full Version (via InformationalVersion).
$assemblyVersion = "$versionPrefix.$buildNumber"
$assemblyInfo = $versionRegex.Replace($assemblyInfo, '<Version>' + $packageVersion + '</Version>')
$assemblyInfo = [Text.RegularExpressions.Regex]::Replace($assemblyInfo, '(<BuildNumber[^>]*>)\d+(</BuildNumber>)', '${1}' + $buildNumber + '${2}')

[IO.File]::WriteAllText($projectFile, $assemblyInfo)

Write-Host "AssemblyVersion=$assemblyVersion, PackageVersion=$packageVersion"
Write-Output $packageVersion