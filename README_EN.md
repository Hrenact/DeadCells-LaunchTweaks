# DeadCells-LaunchTweaks

Optional launch parameters for the Steam Windows version of **Dead Cells**, designed to streamline startup and declutter the main menu.

[中文文档](README.md) · [Download the latest release](https://github.com/Hrenact/DeadCells-LaunchTweaks/releases/latest)

> This is an unofficial community utility. It is not affiliated with or endorsed by Motion Twin, Evil Empire, or Valve.

## Features

| Launch option | Effect |
| --- | --- |
| `-All` | Enables every feature listed below |
| `-SkipIntro` | Skips the Motion Twin / Evil Empire startup sequence |
| `-NoNewsWindow` | Hides the delayed online news and advertising panel |
| `-NoControllerWarning` | Hides the flashing controller recommendation at the bottom of the menu |
| `-NoDailyLeaderboard` | Hides the Daily Challenge leaderboard beside the Play menu |
| `-NoTitleBanner` | Hides the promotional or update banner below the logo without moving the menu |

Options are case-insensitive and may be combined or repeated. If `-All` is present, every feature is enabled. Unrecognized arguments are forwarded to the game unchanged.

## Before and after

Screenshots have not been added yet. Each cell below shows its expected filename. See the [image directory guide](docs/images/README.md) when replacing the placeholders.

### Skip the startup sequence: `-SkipIntro`

| Original | With the option enabled |
| --- | --- |
| **Screenshot placeholder**<br>`docs/images/skip-intro-before.png` | **Screenshot placeholder**<br>`docs/images/skip-intro-after.png` |

Skips the studio-logo wait and reaches the main menu sooner. Normal game initialization still runs.

### Hide online news: `-NoNewsWindow`

| Original | With the option enabled |
| --- | --- |
| **Screenshot placeholder**<br>`docs/images/no-news-window-before.png` | **Screenshot placeholder**<br>`docs/images/no-news-window-after.png` |

Removes the online news area that appears shortly after entering the main menu, including the “Play Windblown now!” promotion.

### Hide the controller recommendation: `-NoControllerWarning`

| Original | With the option enabled |
| --- | --- |
| **Screenshot placeholder**<br>`docs/images/no-controller-warning-before.png` | **Screenshot placeholder**<br>`docs/images/no-controller-warning-after.png` |

Removes the flashing red controller recommendation shown when no controller is connected.

### Hide the Daily Challenge leaderboard: `-NoDailyLeaderboard`

| Original | With the option enabled |
| --- | --- |
| **Screenshot placeholder**<br>`docs/images/no-daily-leaderboard-before.png` | **Screenshot placeholder**<br>`docs/images/no-daily-leaderboard-after.png` |

Hides the leaderboard panel and suppresses its data refresh. The Daily Challenge menu entry remains available.

### Hide the title banner: `-NoTitleBanner`

| Original | With the option enabled |
| --- | --- |
| **Screenshot placeholder**<br>`docs/images/no-title-banner-before.png` | **Screenshot placeholder**<br>`docs/images/no-title-banner-after.png` |

Hides the promotional or update banner below the game logo while preserving its layout space, so the menu does not move upward.

## Installation

1. Download the latest ZIP from [Releases](https://github.com/Hrenact/DeadCells-LaunchTweaks/releases/latest).
2. Fully extract it, close Dead Cells, and run `Install.cmd`.
3. In Steam, open **Dead Cells > Properties > General**.
4. Enter the following in **Launch Options**:

   ```text
   -All
   ```

   You can also enable only selected features:

   ```text
   -SkipIntro -NoNewsWindow -NoTitleBanner
   ```

5. Launch the game normally through Steam.

The installer scans the main Steam directory and all configured Steam library folders. If automatic detection fails, it asks you to select the folder containing `deadcells.exe`.

### Updating and migration

Close the game and run the new `Install.cmd`; uninstalling first is not required. The installer updates only executables it can identify as an earlier version of this proxy. It will not overwrite an unknown executable. Existing `DeadCells-NoNewsWindow` installations can be upgraded directly.

## Uninstallation

1. Close Dead Cells.
2. Run `Uninstall.cmd` to restore the original executables.
3. Remove the custom parameters from the Steam Launch Options field.

## How it works

The installer first preserves the original launchers as:

```text
deadcells.original.exe
deadcells_gl.original.exe
```

It then places a small proxy under the original filenames. When Steam starts the proxy, it reads the custom options, creates the real game process in a suspended state, validates the expected bytecode and live memory, applies the requested changes to that process only, and resumes it. The original executable on disk, `res.pak`, Workshop content, and save files are not modified.

- `-SkipIntro` changes the startup-sequence skip condition.
- `-NoNewsWindow` replaces the news request path with an unused path of equal length.
- `-NoControllerWarning` replaces the localized recommendation text with spaces.
- `-NoDailyLeaderboard` keeps the leaderboard hidden and skips its data refresh.
- `-NoTitleBanner` sets the banner alpha to zero and refreshes its render state while retaining the layout object.
- `-All` expands to all of the switches above inside the proxy.

The proxy waits for the real game to exit, allowing Steam presence, playtime tracking, and the overlay to follow the game normally.

## Compatibility and safety

- Requires 64-bit Windows and the Steam version of Dead Cells.
- Supports both the default renderer and the OpenGL launch choice.
- The current release was tested with Steam v35, build 31 (`8b5231c`, 2026-06-16).
- A game update may change the internal bytecode. The proxy verifies signatures and live memory first, and stops with an error on an unsupported build instead of guessing where to write.
- Before verifying game files or installing a game update, run `Uninstall.cmd`, then reinstall the proxy afterward.
- Because the utility writes to the game process it creates, some security products may inspect it more closely. The complete C# and PowerShell source is included and requires no third-party runtime libraries.

## Building from source

See [BUILD.md](BUILD.md). The project uses the .NET Framework compiler included with Windows and requires no NuGet packages or downloaded dependencies.

## License

Released under the [MIT License](LICENSE). Dead Cells and related names belong to their respective owners.
