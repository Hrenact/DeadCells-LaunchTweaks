# DeadCells-LaunchTweaks

An unofficial Windows launcher proxy for the Steam version of **Dead Cells**.
It adds optional launch parameters that streamline startup and declutter the
main menu without modifying the original game executable on disk.

一个适用于 Steam Windows 版《Dead Cells》的非官方启动代理。它添加了
`-All`、`-SkipIntro`、`-NoNewsWindow`、`-NoControllerWarning`、
`-NoDailyLeaderboard` 和 `-NoTitleBanner` 启动参数，用于精简主菜单和启动流程。

## 中文说明

### 安装

1. 从 [Releases](../../releases) 下载最新版 ZIP，并完整解压。
2. 关闭《Dead Cells》。
3. 双击 `Install.cmd`。
4. 在 Steam 库中右键 **Dead Cells**，选择 **属性 > 通用**。
5. 在“启动选项”中按需输入：

   ```text
   -All
   ```

   `-All` 会一次启用下面的全部功能。也可以按需填写一个或多个单独参数：

   - `-SkipIntro`：跳过 Motion Twin / Evil Empire 启动画面。
   - `-NoNewsWindow`：隐藏延后出现的在线新闻区域。
   - `-NoControllerWarning`：隐藏菜单底部闪烁的控制器建议文字。
   - `-NoDailyLeaderboard`：隐藏“游玩”菜单右侧的每日挑战排行榜面板。
   - `-NoTitleBanner`：隐藏主界面游戏 Logo 下方的宣传/更新横幅。

6. 此后正常从 Steam 启动游戏。

安装器会自动扫描 Steam 及所有库盘。如果无法定位游戏，它会要求你选择包含
`deadcells.exe` 的游戏目录。

从旧版升级时无需先卸载；关闭游戏后直接运行新版 `Install.cmd` 即可。安装器只会
原位替换能够确认身份的旧版代理，不会覆盖来源不明的 EXE。

### 卸载

1. 关闭游戏。
2. 双击 `Uninstall.cmd`。
3. 从 Steam 启动选项中删除这些自定义参数。

### 工作方式

安装器把原版启动程序保留为：

```text
deadcells.original.exe
deadcells_gl.original.exe
```

随后放入同名的小型启动代理。Steam 将启动选项传给代理：

- 检测到 `-SkipIntro` 时，代理会先以挂起状态启动原版程序，在本次进程的内存中
  将启动画面的输入判断改为“跳过”，然后恢复运行。磁盘上的原版程序不会被改写。
- 检测到 `-NoNewsWindow` 时，代理会仅在本次游戏进程内将新闻请求路径替换为
  未使用的路径。
- 检测到 `-NoControllerWarning` 时，代理会仅在本次游戏进程内将控制器建议的
  本地化键替换为等长空白，不改变手柄检测或其他提示。
- 检测到 `-NoDailyLeaderboard` 时，代理会让排行榜面板始终保持隐藏，并跳过其
  数据刷新；“每日挑战”菜单入口仍然可用。
- 检测到 `-NoTitleBanner` 时，代理保留横幅对象和原有布局高度，但将其透明度固定为 0。

不带任何自定义参数时，游戏会按原版方式运行。

代理会等待实际游戏退出，因此 Steam 游戏状态和覆盖层可以正常跟随游戏。
它不会修改 `res.pak`、Workshop 内容或存档。

## English

### Installation

1. Download and fully extract the latest ZIP from [Releases](../../releases).
2. Close Dead Cells and run `Install.cmd`.
3. Open **Dead Cells > Properties > General > Launch Options** in Steam.
4. Enter any combination of `-SkipIntro`, `-NoNewsWindow`,
   `-NoControllerWarning`, `-NoDailyLeaderboard`, and `-NoTitleBanner`, or use
   `-All` to enable every feature, then launch normally.

The installer scans the main Steam folder and all Steam library folders. If
automatic detection fails, it opens a folder picker.

### Uninstallation

Close the game, run `Uninstall.cmd`, and remove the custom options from the
Steam launch options field.

## Compatibility and safety

- Windows x64 and the Steam version of Dead Cells are required.
- Both the default renderer and the OpenGL launch choice are supported.
- The memory patches are bytecode-signature checked and may need an update
  after a Dead Cells update. Version 1.0.0 was tested with Steam v35, build 31
  (`8b5231c`, 2026-06-16).
- Uninstall before verifying game files or installing a game update, then
  install the proxy again afterward.
- The launcher validates the original executable layout and live memory bytes.
  It stops with an error on an incompatible build instead of guessing.
- Process-memory tools may receive additional scrutiny from antivirus products.
  The complete C# and PowerShell source is included and can be rebuilt using
  only the .NET Framework compiler bundled with Windows.
- This project contains no files from Dead Cells.

## Building

See [BUILD.md](BUILD.md). No NuGet packages or downloaded dependencies are
required.

## Disclaimer

This is an unofficial community utility. It is not affiliated with or endorsed
by Motion Twin, Evil Empire, or Valve. Dead Cells and related names are the
property of their respective owners.

## License

The launcher and installation scripts are available under the [MIT License](LICENSE).
