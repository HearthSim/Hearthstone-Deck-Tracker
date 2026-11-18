Param(
    [Parameter(Mandatory=$True)]
    [string]$packageVersion,
    [ValidateSet("velopack", "dev")]
    $type,
    [string]$repo
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

if (!$repo) {
    $repo = @{
        dev = "HDT-dev-builds"
        velopack = "HDT-Releases"
    }[$type]
}

Write-Host "Creating $type release $packageVersion on $repo..."

$releaseBody = @{
    dev = @"
# Development build (unstable)
### Using this build is not recommended!
### Download the latest stable build here: https://hsreplay.net/downloads/
"@
    velopack = ""
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

$baseDir = @{
    dev = "dev-builds"
    velopack = "velopack-builds"
}[$type]

$assetFilter = @{
    dev = "HDT-Installer.exe", "Hearthstone.Deck.Tracker-*.zip", "*.nupkg", "RELEASES", "releases.win.json"
    velopack = "HDT-Installer.exe", "*.nupkg", "RELEASES", "releases.win.json"
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
