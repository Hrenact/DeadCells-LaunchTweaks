function Test-DeadCellsDirectory {
    param([string]$Path)

    if ([string]::IsNullOrWhiteSpace($Path)) { return $false }
    return (Test-Path -LiteralPath (Join-Path $Path 'deadcells.exe') -PathType Leaf) -or
           (Test-Path -LiteralPath (Join-Path $Path 'deadcells.original.exe') -PathType Leaf)
}

function Add-UniquePath {
    param(
        [System.Collections.Generic.List[string]]$List,
        [string]$Path
    )

    if ([string]::IsNullOrWhiteSpace($Path)) { return }
    try { $resolved = [IO.Path]::GetFullPath($Path.Trim()) } catch { return }
    foreach ($existing in $List) {
        if ([string]::Equals($existing, $resolved, [StringComparison]::OrdinalIgnoreCase)) { return }
    }
    $List.Add($resolved)
}

function Get-SteamLibraryRoots {
    $steamRoots = [System.Collections.Generic.List[string]]::new()
    $registryKeys = @(
        'HKCU:\Software\Valve\Steam',
        'HKLM:\SOFTWARE\WOW6432Node\Valve\Steam',
        'HKLM:\SOFTWARE\Valve\Steam'
    )

    foreach ($key in $registryKeys) {
        try {
            $values = Get-ItemProperty -LiteralPath $key -ErrorAction Stop
            Add-UniquePath -List $steamRoots -Path $values.SteamPath
            if ($values.InstallPath) { Add-UniquePath -List $steamRoots -Path $values.InstallPath }
            if ($values.SteamExe) { Add-UniquePath -List $steamRoots -Path ([IO.Path]::GetDirectoryName($values.SteamExe)) }
        } catch { }
    }

    $libraries = [System.Collections.Generic.List[string]]::new()
    foreach ($steamRoot in @($steamRoots)) {
        Add-UniquePath -List $libraries -Path $steamRoot
        $vdf = Join-Path $steamRoot 'steamapps\libraryfolders.vdf'
        if (-not (Test-Path -LiteralPath $vdf -PathType Leaf)) { continue }
        try {
            $text = [IO.File]::ReadAllText($vdf)
            foreach ($match in [regex]::Matches($text, '"path"\s+"([^"]+)"', 'IgnoreCase')) {
                Add-UniquePath -List $libraries -Path ($match.Groups[1].Value -replace '\\\\', '\')
            }
            foreach ($match in [regex]::Matches($text, '(?m)^\s*"\d+"\s+"([^"]+)"')) {
                Add-UniquePath -List $libraries -Path ($match.Groups[1].Value -replace '\\\\', '\')
            }
        } catch { }
    }
    return $libraries.ToArray()
}

function Find-DeadCellsFromSteam {
    $results = [System.Collections.Generic.List[string]]::new()
    foreach ($library in Get-SteamLibraryRoots) {
        $manifest = Join-Path $library 'steamapps\appmanifest_588650.acf'
        if (-not (Test-Path -LiteralPath $manifest -PathType Leaf)) { continue }
        try {
            $text = [IO.File]::ReadAllText($manifest)
            $match = [regex]::Match($text, '"installdir"\s+"([^"]+)"', 'IgnoreCase')
            if (-not $match.Success) { continue }
            $candidate = Join-Path (Join-Path $library 'steamapps\common') $match.Groups[1].Value
            if (Test-DeadCellsDirectory $candidate) { Add-UniquePath -List $results -Path $candidate }
        } catch { }
    }
    return $results.ToArray()
}

function Select-DeadCellsDirectory {
    Add-Type -AssemblyName System.Windows.Forms
    $dialog = New-Object System.Windows.Forms.FolderBrowserDialog
    $dialog.Description = 'Select the Dead Cells installation folder (the folder containing deadcells.exe).'
    $dialog.ShowNewFolderButton = $false
    if ($dialog.ShowDialog() -ne [System.Windows.Forms.DialogResult]::OK) {
        throw 'Dead Cells folder selection was cancelled.'
    }
    if (-not (Test-DeadCellsDirectory $dialog.SelectedPath)) {
        throw "The selected folder does not contain Dead Cells: $($dialog.SelectedPath)"
    }
    return [IO.Path]::GetFullPath($dialog.SelectedPath)
}

function Resolve-DeadCellsDirectory {
    param([string]$RequestedPath)

    if (-not [string]::IsNullOrWhiteSpace($RequestedPath)) {
        if (-not (Test-DeadCellsDirectory $RequestedPath)) {
            throw "Dead Cells was not found at the specified path: $RequestedPath"
        }
        return [IO.Path]::GetFullPath($RequestedPath)
    }

    $packageParent = Split-Path -Parent $PSScriptRoot
    if (Test-DeadCellsDirectory $packageParent) {
        return [IO.Path]::GetFullPath($packageParent)
    }

    $detected = @(Find-DeadCellsFromSteam)
    if ($detected.Count -eq 1) { return $detected[0] }
    if ($detected.Count -gt 1) {
        Write-Host 'Multiple Dead Cells installations were detected:'
        $detected | ForEach-Object { Write-Host "  $_" }
    } else {
        Write-Host 'Dead Cells was not detected automatically.'
    }
    return Select-DeadCellsDirectory
}
