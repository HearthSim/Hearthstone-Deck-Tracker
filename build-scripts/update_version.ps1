Param(
    [Parameter(Mandatory=$True)]
    [int]$buildNumber,
    [Parameter(Mandatory=$True)]
    [boolean]$dev
)

$baseDir = $(Resolve-Path "$PSScriptRoot\..").Path
$assemblyInfoFile = "$baseDir\Hearthstone Deck Tracker\Properties\AssemblyInfo.cs"

# Read version number from AssemblyInfo
$assemblyInfo = [IO.File]::ReadAllText($assemblyInfoFile)
$versionRegex = New-Object System.Text.RegularExpressions.Regex('Version\(\"(\d+)\.(\d+)\.(\d+)"\)')
$match = $versionRegex.Match($assemblyInfo)
if(!$match.Success) {
    throw "Version number not found in AssemblyInfo"
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
$assemblyInfo = $versionRegex.Replace($assemblyInfo, 'Version("' + $assemblyVersion + '")')
[IO.File]::WriteAllText($assemblyInfoFile, $assemblyInfo)

Write-Host "AssemblyVersion=$assemblyVersion, PackageVersion=$packageVersion"
Write-Output $packageVersion