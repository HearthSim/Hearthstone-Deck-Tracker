$assemblyVersion = Get-Content "Hearthstone Deck Tracker\Properties\AssemblyInfo.cs" | 
    where { $_ -match '\[assembly: AssemblyVersion\("([\.\d]+)"\)\]' } |
    foreach { $matches[1] }
    
if ($assemblyVersion.EndsWith(".0")) {
    $assemblyVersion = $assemblyVersion.Substring(0, $assemblyVersion.Length - 2)
}

$baseDir = "Hearthstone Deck Tracker\Releases"
$targetDir = "$baseDir\v$assemblyVersion"
if (Test-Path $targetDir) {
    echo "build_release.ps1: build error 1: directory '$targetDir' already exists"
    return
}

msbuild /p:Configuration=Release
msbuild /p:Configuration=Squirrel

mkdir $targetDir
cp "$baseDir\RELEASES" $targetDir
cp "$baseDir\Setup.exe" "$targetDir\HDT-Installer.exe"
cp "$baseDir\*-delta.nupkg" $targetDir
cp "$baseDir\*$assemblyVersion-full.nupkg" $targetDir
cp "$baseDir\Hearthstone.Deck.Tracker-v$assemblyVersion.zip" $targetDir
