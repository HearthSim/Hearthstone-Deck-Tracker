Param(
    [Parameter(Mandatory=$True)]
    [string]$dir,
    [Parameter(Mandatory=$True)]
    [string]$bucket
)

$libs = @(
    "HSReplay.dll",
    "BobsBuddy.dll",
    "BobsBuddy.Common.dll",
    "HearthDb.dll",
    "HearthMirror.dll",
    "untapped-scry-dotnet.dll"
)

function Assert-Signed([string]$file) {
    $signature = Get-AuthenticodeSignature $file
    if ($signature.Status -ne "Valid") {
        throw "$file is not validly signed ($($signature.Status))"
    }
}

foreach ($lib in $libs) {
    $file = Join-Path $dir $lib
    $name = [IO.Path]::GetFileNameWithoutExtension($lib)
    # key by the unsigned bytes so a new upstream build is a guaranteed miss
    $hash = (Get-FileHash $file -Algorithm SHA256).Hash.ToLower()
    $key = "signed/$name/$name.$hash.dll"
    $cached = Join-Path $env:RUNNER_TEMP "$name.$hash.dll"

    $output = aws s3api get-object --bucket $bucket --key $key $cached 2>&1 | Out-String
    if ($LASTEXITCODE -eq 0) {
        "Using cached signed $lib ($key)"
        Assert-Signed $cached
        Copy-Item $cached $file -Force
        continue
    }
    # anything but a missing key (e.g. AccessDenied) must not silently fall through to signing
    if ($output -notmatch "NoSuchKey") {
        throw "Failed to look up $key in $bucket`n$output"
    }

    "Signing $lib ($key)"
    smctl sign --simple --keypair-alias=key_1409653344 --input="$file"
    Assert-Signed $file
    aws s3api put-object --bucket $bucket --key $key --body $file
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to upload $key to $bucket"
    }
}
