Param(
    [Parameter(Mandatory=$True)]
    [string]$packageVersion,
    [ValidateSet("squirrel", "portable", "dev")]
    $type
)

if ($type -eq "dev" -and !$packageVersion.Contains("dev")) {
    throw "packageVersion must be a dev release for type=dev"
}
if ($type -ne "dev" -and $packageVersion.Contains("dev")) {
    throw "packageVersion must be a non-dev release for type=$type"
}

$authHeader = @{
    Authorization = "token $env:HDT_GITHUB_TOKEN"
}

Write-Host "Creating $type release $packageVersion..."

$repo = @{
    dev = "HDT-dev-builds"
    portable = "Hearthstone-Deck-Tracker"
    squirrel = "HDT-Releases"
}[$type]

$releaseBody = @{
    dev = @"
# Development build (unstable)
### Using this build is not recommended!
### Download the latest stable build here: https://hsreplay.net/downloads/
"@
    portable = @"
## Download here: https://hsreplay.net/downloads/
<Insert Release Notes Here>
"@
    squirrel = ""
}[$type]

$body = @{
    tag_name = "v$packageVersion"
    name = "v$packageVersion"
    draft = $true
    body = $releaseBody
} | ConvertTo-Json

$params = @{
    Uri = "https://api.github.com/repos/HearthSim/$repo/releases"
    Method = "POST"
    Body = $body
    UseBasicParsing = $true
    Headers = $authHeader
}

$response = Invoke-WebRequest @params
if ($response.StatusCode -ne 201) {
    throw "Received unexected StatusCode when creating release: $($response.StatusCode)"
}

Write-Host "Successfully created release"

$responseContent = $response.Content | ConvertFrom-Json

$baseDir = if ($type -eq "dev") {"dev-builds"} else {"builds"}

$assetFilter = @{
    dev = "HDT-Installer.exe", "Hearthstone.Deck.Tracker-*.zip", "*.nupkg", "RELEASES"
    portable = "Hearthstone.Deck.Tracker-*.zip"
    squirrel = "HDT-Installer.exe", "*.nupkg", "RELEASES"
}[$type]

$assets = Get-ChildItem "$baseDir\$packageVersion\*" -Include $assetFilter
Write-Host "`nUploading $($assets.Length) assets..."

$assets | ForEach-Object {
    Write-Host "Uploading $($_.Name)..."
    $params = @{
        Uri = $responseContent.upload_url -replace "{.+}$", "?name=$($_.Name)"
        Method = "POST"
        ContentType = "application/zip"
        Headers = $authHeader
        UseBasicParsing = $true
        InFile = $_.FullName
    }
    $uploadResponse = Invoke-WebRequest @params
    if ($uploadResponse.StatusCode -ne 201) {
        throw "Received unexected StatusCode when uploading $($_.Name): $($response.StatusCode)"
    }
    Write-Host "Successfully uploaded $($_.Name)"
}

Write-host "`nDone."
