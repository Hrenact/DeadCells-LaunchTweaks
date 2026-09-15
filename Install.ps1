[CmdletBinding()]
param([string]$GamePath)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'LaunchTweaks.Common.ps1')

$GamePath = Resolve-DeadCellsDirectory $GamePath
$proxy = Join-Path $PSScriptRoot 'DeadCellsLaunchProxy.exe'
$names = @('deadcells.exe', 'deadcells_gl.exe')

Write-Host "Dead Cells directory: $GamePath"
if (Get-Process deadcells, deadcells_gl, 'deadcells.original', 'deadcells_gl.original' -ErrorAction SilentlyContinue) {
    throw 'Dead Cells is running. Close it before installing.'
}
if (-not (Test-Path -LiteralPath $proxy -PathType Leaf)) {
    throw "Proxy executable is missing: $proxy"
}

$proxyHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $proxy).Hash
$plan = @()
foreach ($name in $names) {
    $target = Join-Path $GamePath $name
    $stem = [IO.Path]::GetFileNameWithoutExtension($name)
    $original = Join-Path $GamePath ($stem + '.original.exe')
    if (-not (Test-Path -LiteralPath $target -PathType Leaf)) {
        throw "Game executable not found: $target"
    }

    $targetHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $target).Hash
    if (Test-Path -LiteralPath $original -PathType Leaf) {
        if ($targetHash -eq $proxyHash) {
            $plan += [pscustomobject]@{ Target = $target; Original = $original; Action = 'None' }
            continue
        }

        $originalFileName = (Get-Item -LiteralPath $target).VersionInfo.OriginalFilename
        if ($originalFileName -ne 'DeadCellsLaunchProxy.exe') {
            throw "Ambiguous install state for $name. Uninstall before updating, or verify the game files and remove the stale .original.exe backup."
        }
        $plan += [pscustomobject]@{ Target = $target; Original = $original; Action = 'Update' }
    } else {
        $plan += [pscustomobject]@{ Target = $target; Original = $original; Action = 'Install' }
    }
}

foreach ($item in $plan) {
    if ($item.Action -eq 'None') {
        Write-Host "$([IO.Path]::GetFileName($item.Target)) : already installed"
        continue
    }
    if ($item.Action -eq 'Update') {
        Copy-Item -LiteralPath $proxy -Destination $item.Target -Force
        Write-Host "$([IO.Path]::GetFileName($item.Target)) : updated"
        continue
    }
    Move-Item -LiteralPath $item.Target -Destination $item.Original
    try {
        Copy-Item -LiteralPath $proxy -Destination $item.Target
    } catch {
        Move-Item -LiteralPath $item.Original -Destination $item.Target
        throw
    }
    Write-Host "$([IO.Path]::GetFileName($item.Target)) : installed"
}

Write-Host ''
Write-Host 'Installation complete.' -ForegroundColor Green
Write-Host 'Steam launch options: -All, or any combination of -SkipIntro -NoNewsWindow -NoControllerWarning -NoDailyLeaderboard -NoTitleBanner'
