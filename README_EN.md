# DeadCells-LaunchTweaks

Optional launch parameters for the Steam Windows version of **Dead Cells**, designed to streamline startup and declutter the main menu.

[中文文档](README.md) · [Download the latest release](https://github.com/Hrenact/DeadCells-LaunchTweaks/releases/latest)

> This is an unofficial community utility. It is not affiliated with or endorsed by Motion Twin, Evil Empire, or Valve.

## Feature overview

| Launch option | Effect |
| --- | --- |
| `-All` | Enables all features listed below at once |
| `-SkipIntro` | Reduces the time spent waiting on the startup screens |
| `-NoNewsWindow` | Hides the delayed online news and advertising panel |
| `-NoControllerWarning` | Hides the flashing controller recommendation at the bottom of the menu |
| `-NoDailyLeaderboard` | Hides the Daily Challenge leaderboard beside the Play menu |
| `-NoTitleBanner` | Hides the promotional or update banner below the main-menu logo while preserving the menu layout |

Options are case-insensitive and may be combined or repeated. If `-All` is present, every feature is enabled.
Unrecognized arguments are forwarded to the game unchanged.

## Before and after

### Enable all features

```text
-All
```

Enables every feature at once—ideal for veteran players who want the cleanest experience without typing every option.

| Original | With the option enabled |
| --- | --- |
| <img src="docs/images/oiginal.webp" alt="Original main menu and Play menu" width="100%"> <br> <img src="docs/images/oiginal1.webp" alt="Original Play submenu" width="100%"> | <img src="docs/images/all.webp" alt="Main menu with all tweaks enabled" width="100%"> <br> <img src="docs/images/all1.webp" alt="Play submenu with all tweaks enabled" width="100%"> |

### Shorten the startup screens

```text
-SkipIntro
```

Reduces the studio-logo wait and reaches the main menu sooner. Normal game initialization still runs.

| Original | With the option enabled |
| --- | --- |

https://github.com/user-attachments/assets/fc9c8671-2aff-4b0c-9ccc-8115b286870f

### Hide online news and advertising

```text
-NoNewsWindow
```

Removes the online news area and promotional content that appear shortly after entering the main menu.

| Original | With the option enabled |
| --- | --- |
| <img src="docs/images/oiginal.webp" alt="Original main menu with online news" width="100%"> | <img src="docs/images/no-news-window.webp" alt="Main menu without online news" width="100%"> |

### Hide the controller recommendation: `-NoControllerWarning`

| Original | With the option enabled |
| --- | --- |
| <img src="docs/images/oiginal.webp" alt="Original main menu with controller recommendation" width="100%"> | <img src="docs/images/no-controller-warning.webp" alt="Main menu without controller recommendation" width="100%"> |

Removes the flashing red controller recommendation shown at the bottom of the menu when no controller is connected.

### Hide the Daily Challenge leaderboard

```text
-NoDailyLeaderboard
```

Hides the Daily Challenge leaderboard beside the Play menu and suppresses its data refresh. The Daily Challenge menu entry remains available.

| Original | With the option enabled |
| --- | --- |
| <img src="docs/images/DailyLeaderboard.webp" alt="Play menu with Daily Challenge leaderboard" width="100%"> | <img src="docs/images/no-daily-leaderboard.webp" alt="Play menu without Daily Challenge leaderboard" width="100%"> |

### Hide the title banner

```text
-NoTitleBanner
```

Hides the promotional or update banner below the game logo while retaining its original layout space, so the menu does not move upward.

| Original | With the option enabled |
| --- | --- |
| <img src="docs/images/oiginal.webp" alt="Original main menu with title banner" width="100%"> | <img src="docs/images/no-title-banner.webp" alt="Main menu without title banner" width="100%"> |

## Installation

1. Download the latest ZIP from [Releases](https://github.com/Hrenact/DeadCells-LaunchTweaks/releases/latest).
2. Fully extract the ZIP, close Dead Cells, and run `Install.cmd`.
3. In Steam, right-click **Dead Cells** and open **Properties > General**.
4. Enter the following in **Launch Options**:

   ```text
   -All
   ```

   You can also enable only selected features, for example:

   ```text
   -SkipIntro -NoNewsWindow -NoTitleBanner
   ```

5. Launch the game normally through Steam from then on.

The installer scans the main Steam directory and all other configured Steam library folders. If automatic detection fails, it asks you to select the folder containing `deadcells.exe`.

### Updating from an earlier version

Close the game and run the new `Install.cmd`; uninstalling first is not required. The installer replaces only an earlier proxy executable whose identity it can verify. It will not overwrite an executable from an unknown source. Existing `DeadCells-NoNewsWindow` installations can be upgraded directly.

## Uninstallation

1. Close Dead Cells.
2. Run `Uninstall.cmd` and wait for the original launchers to be restored.
3. Remove this project's custom parameters from the Steam Launch Options field.

## How it works

The installer does not overwrite the only copy of the game launchers. It first preserves the originals as:

```text
deadcells.original.exe
deadcells_gl.original.exe
```

It then places a small launch proxy under the original filenames. When Steam starts the proxy, it reads the custom options, creates the real game process in a suspended state, applies signature-verified changes to that process's memory only, and resumes the game. The original executables on disk, `res.pak`, Workshop content, and save files are not modified.

- `-SkipIntro` changes the startup-screen skip condition.
- `-NoNewsWindow` replaces the news request path with an unused path of equal length.
- `-NoControllerWarning` replaces the localized controller recommendation with spaces.
- `-NoDailyLeaderboard` keeps the leaderboard hidden and skips its data refresh.
- `-NoTitleBanner` sets the banner alpha to zero and refreshes its render state while retaining the layout object.
- `-All` expands to all of the switches above inside the proxy.

The proxy waits for the real game to exit, allowing Steam presence, playtime tracking, and the overlay to follow the game normally.

## Compatibility and safety

- Requires 64-bit Windows and the Steam version of Dead Cells.
- Supports both Steam's default renderer and the OpenGL launch choice.
- The current release was tested with Steam v35, build 31 (`8b5231c`, 2026-06-16).
- A game update may change the internal bytecode. The proxy verifies signatures and live memory first, and stops with an error on an unsupported build instead of guessing where to write.
- Before updating the game or verifying its files, run `Uninstall.cmd`; reinstall the proxy afterward.
- Because the utility writes to the game process it creates, some security products may inspect it more closely. The repository contains the complete C# and PowerShell source and requires no third-party runtime libraries.

## Building from source

See [BUILD.md](BUILD.md). The project uses only the .NET Framework compiler included with Windows and requires no NuGet packages or additional downloaded dependencies.

## License

Released under the [MIT License](LICENSE). Dead Cells and related names belong to their respective owners.
