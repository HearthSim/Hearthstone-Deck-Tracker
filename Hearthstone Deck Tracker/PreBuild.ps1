param(
	[Parameter(Mandatory=$true)]
	[string]$solution,

	[Parameter(Mandatory=$true)]
	[string]$project,

	[Parameter(Mandatory=$true)]
	[string]$target,

	[switch]$skipGitSync = $false,
	[switch]$skipResourceCopy = $false
)

"Solution: $solution"
"Project: $project"
"Target: $target"
"Skip git sync: $skipGitSync"
"Skip resource copy: $skipResourceCopy`n"

function CopyFiles($source, $files, $target) {
	if((Test-Path $source) -and (Test-Path $target)) {
		"`nCopying files from $source to $target"
		Copy-Item -Force -Recurse "$source\$files" $target
	}
}

function Create($path) {
	if(-not (Test-Path $path)) {
		"`nCreating directory: $path"
		New-Item -ItemType "directory" -Path $path
	}
}

function SyncRepo($name, $localDir, $branch = "origin/master") {
	$localPath = "$solution$localDir"
	if(Test-Path $localPath)	{
		"`nResetting $localDir to $branch"
		git -C $localPath fetch
		git -C $localPath reset --hard $branch
	} else {
		git clone --depth 1 "https://github.com/$name.git" $localPath
	}
}

function FetchLib($name) {
	"Fetching $name..."
	$url = "https://libs.hearthsim.net/hdt/$name.dll"
	(New-Object Net.WebClient).DownloadFile($url, "$solution\lib\$name.dll")
}

if(-not $skipGitSync) {
	FetchLib "HearthDb"
	FetchLib "HearthMirror"
	FetchLib "HSReplay"
	FetchLib "BobsBuddy"
	SyncRepo "HearthSim/HDT-Localization" "HDT-Localization"
}

if(-not $skipResourceCopy) {
	CopyFiles "$($solution)HDT-Localization" "*.resx" "$($project)Properties\"

	$images = "$($target)Images"
	Create "$images\Tiles"
	Create "$images\Themes"

	CopyFiles "$($solution)Resources\Generated\Tiles" "*" "$images\Tiles"
	CopyFiles "$($project)\Images\Themes" "*" "$images\Themes"
	CopyFiles "$($solution)" "CHANGELOG.md" "$($project)Resources"
}

"`nDone."
