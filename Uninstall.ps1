[CmdletBinding()]
param([string]$GamePath)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'LaunchTweaks.Common.ps1')

$GamePath = Resolve-DeadCellsDirectory $GamePath
$proxy = Join-Path $PSScriptRoot 'DeadCellsLaunchProxy.exe'
$names = @('deadcells.exe', 'deadcells_gl.exe')

Write-Host "Dead Cells directory: $GamePath"
if (Get-Process deadcells, deadcells_gl, 'deadcells.original', 'deadcells_gl.original' -ErrorAction SilentlyContinue) {
    throw 'Dead Cells is running. Close it before uninstalling.'
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
    $hasTarget = Test-Path -LiteralPath $target -PathType Leaf
    $hasOriginal = Test-Path -LiteralPath $original -PathType Leaf

    if (-not $hasOriginal) {
        if (-not $hasTarget) { throw "Game executable not found: $target" }
        $plan += [pscustomobject]@{ Target = $target; Original = $original; Installed = $false }
        continue
    }
    if (-not $hasTarget) {
        throw "Proxy executable is missing while its original backup exists: $target"
    }
    if ((Get-FileHash -Algorithm SHA256 -LiteralPath $target).Hash -ne $proxyHash) {
        throw "The current $name is not this release's proxy. No files were changed."
    }
    $plan += [pscustomobject]@{ Target = $target; Original = $original; Installed = $true }
}

foreach ($item in $plan) {
    if (-not $item.Installed) {
        Write-Host "$([IO.Path]::GetFileName($item.Target)) : not installed"
        continue
    }
    Remove-Item -LiteralPath $item.Target
    Move-Item -LiteralPath $item.Original -Destination $item.Target
    Write-Host "$([IO.Path]::GetFileName($item.Target)) : restored"
}

Write-Host ''
Write-Host 'Uninstall complete. Remove the LaunchTweaks parameters from Steam launch options.' -ForegroundColor Green
