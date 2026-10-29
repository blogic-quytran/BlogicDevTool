# Publishes a new BLogicDevTool release on GitHub; running copies pick it up via UpdateService.
#   .\release.ps1 -Description "Fix DB restore"         -> bump patch (1.0.3 -> 1.0.4)
#   .\release.ps1 -Version 1.2.0 -Description "WebDAV upload" -Notes "- Added X`n- Fixed Y"
# -Description is the one-line summary shown in the app's update banner/dialog.
param(
    [string]$Version,
    [string]$Description,
    [string]$Notes
)

$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
$csproj = Join-Path $PSScriptRoot 'BLogicDevTool.csproj'

function Run([string]$exe, [string[]]$argv) {
    & $exe @argv
    if ($LASTEXITCODE -ne 0) { throw "$exe $($argv -join ' ') failed (exit $LASTEXITCODE)" }
}

if (git status --porcelain) { throw 'Working tree is not clean. Commit or stash your changes first.' }

$xml = [IO.File]::ReadAllText($csproj)
$current = [regex]::Match($xml, '<Version>([^<]+)</Version>').Groups[1].Value
if (-not $Version) {
    $v = [Version]$current
    $Version = "$($v.Major).$($v.Minor).$($v.Build + 1)"
}
if ($Version -notmatch '^\d+\.\d+\.\d+$') { throw "Version must look like 1.2.3 (got '$Version')." }
$tag = "v$Version"
if (git tag --list $tag) { throw "Tag $tag already exists." }

# via cmd: in PowerShell 5.1 a redirected native stderr + ErrorActionPreference=Stop throws
$prevTag = cmd /c 'git describe --tags --abbrev=0 2>nul'
if (-not $Notes) {
    $range = if ($prevTag) { "$prevTag..HEAD" } else { 'HEAD' }
    $Notes = (git log $range --no-merges --pretty=format:'- %s' | Where-Object { $_ -notmatch '^- Release v' }) -join "`n"
    if (-not $Notes) { $Notes = "Release $tag" }
}
if (-not $Description) {
    $Description = git log -1 --no-merges --pretty=format:'%s' --grep='^Release v' --invert-grep
}
$title = if ($Description) { "$tag - $Description" } else { $tag }

Write-Host "Releasing $tag (current csproj version $current, previous tag $prevTag)" -ForegroundColor Cyan

# Bump the version (keep the file's UTF-8 BOM)
if ($current -ne $Version) {
    $xml = $xml -replace '<Version>[^<]+</Version>', "<Version>$Version</Version>"
    [IO.File]::WriteAllText($csproj, $xml, (New-Object Text.UTF8Encoding $true))
}

Get-Process -Name BLogicDevTool -ErrorAction SilentlyContinue | Stop-Process -Force
$out = Join-Path $PSScriptRoot 'publish_release'
if (Test-Path $out) { Remove-Item $out -Recurse -Force }
Run dotnet @('publish', $csproj, '-c', 'Release', '-r', 'win-x64', '--self-contained', 'true',
    '-p:PublishSingleFile=true', '-p:IncludeNativeLibrariesForSelfExtract=true',
    '-p:EnableCompressionInSingleFile=true', '-o', $out, '--nologo')
$exe = Join-Path $out 'BLogicDevTool.exe'
if (-not (Test-Path $exe)) { throw "Publish did not produce $exe" }
# The app verifies the download against this (it reads releases.atom, which carries no digests).
$shaFile = "$exe.sha256"
$hash = (Get-FileHash $exe -Algorithm SHA256).Hash.ToLower()
[IO.File]::WriteAllText($shaFile, "$hash  BLogicDevTool.exe`n", (New-Object Text.UTF8Encoding $false))

if ($current -ne $Version) {
    Run git @('add', 'BLogicDevTool.csproj')
    Run git @('commit', '-m', "Release $tag")
}
Run git @('tag', '-a', $tag, '-m', "Release $tag")
Run git @('push', 'origin', 'HEAD')
Run git @('push', 'origin', $tag)

$notesFile = Join-Path $env:TEMP "blogic_release_notes_$tag.md"
[IO.File]::WriteAllText($notesFile, $Notes, (New-Object Text.UTF8Encoding $false))
try {
    Run gh @('release', 'create', $tag, $exe, $shaFile, '--title', $title, '--notes-file', $notesFile)
} finally {
    Remove-Item $notesFile -Force -ErrorAction SilentlyContinue
}

Write-Host "Done: https://github.com/blogic-quytran/BlogicDevTool/releases/tag/$tag" -ForegroundColor Green
