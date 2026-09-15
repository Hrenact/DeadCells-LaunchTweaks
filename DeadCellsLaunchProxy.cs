using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

[assembly: System.Reflection.AssemblyTitle("DeadCells-LaunchTweaks")]
[assembly: System.Reflection.AssemblyProduct("DeadCells-LaunchTweaks")]
[assembly: System.Reflection.AssemblyDescription("Steam launch proxy for Dead Cells")]
[assembly: System.Reflection.AssemblyVersion("1.0.0.0")]
[assembly: System.Reflection.AssemblyFileVersion("1.0.0.0")]

internal static class DeadCellsLaunchProxy
{
    private const string NoNewsSwitch = "-NoNewsWindow";
    private const string SkipIntroSwitch = "-SkipIntro";
    private const string NoControllerWarningSwitch = "-NoControllerWarning";
    private const string NoDailyLeaderboardSwitch = "-NoDailyLeaderboard";
    private const string NoTitleBannerSwitch = "-NoTitleBanner";
    private const string AllSwitch = "-All";
    private const string SourceText = "/lastNews";
    private const string ReplacementText = "/noNews__";
    private const string ControllerWarningText = "Il est conseillé de jouer à Dead Cells avec une manette !";
    private static readonly byte[] DailyLeaderboardVisibilitySignature =
    {
        0x28, 0x03, 0x19, 0x2C, 0x03, 0x06, 0x2D, 0x01, 0x05
    };
    private static readonly byte[] DailyLeaderboardVisibilityReplacement =
    {
        0x03, 0x01, 0x00, 0x2C, 0x01, 0x06, 0x2D, 0x01, 0x05
    };
    private static readonly byte[] TitleBannerVisibilitySignature =
    {
        0x28, 0x24, 0x20, 0x2E, 0x24, 0x05, 0x28, 0x24, 0x20, 0x47, 0x24,
        0x03, 0x02, 0x01, 0x3B, 0x25, 0x02, 0x27, 0x24, 0x17, 0x25,
        0x01, 0x26, 0x01, 0x3B, 0x27, 0x26, 0x01, 0x26, 0x01
    };
    private static readonly byte[] TitleBannerVisibilityReplacement =
    {
        0x28, 0x24, 0x20, 0x2E, 0x24, 0x05, 0x28, 0x24, 0x20, 0x47, 0x24,
        0x27, 0x24, 0x0A, 0x03, 0x03, 0x02, 0x01, 0x27, 0x24, 0x13, 0x02,
        0x01, 0x26, 0x01, 0x3B, 0x27, 0x26, 0x47, 0x24
    };
    private static readonly byte[] SkipIntroSignature =
    {
        0x04, 0x3A, 0x02, 0x03, 0x04, 0x00, 0x00, 0x11, 0x04, 0x2D,
        0x11, 0x03, 0x03, 0x04, 0x01, 0x3B, 0x14, 0x04, 0x1A, 0x01
    };
    private const int SkipIntroBooleanOffset = 5;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct StartupInfo
    {
        public int Size;
        public string Reserved;
        public string Desktop;
        public string Title;
        public int X;
        public int Y;
        public int XSize;
        public int YSize;
        public int XCountChars;
        public int YCountChars;
        public int FillAttribute;
        public int Flags;
        public short ShowWindow;
        public short Reserved2;
        public IntPtr ReservedPointer;
        public IntPtr StandardInput;
        public IntPtr StandardOutput;
        public IntPtr StandardError;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ProcessInformation
    {
        public IntPtr Process;
        public IntPtr Thread;
        public int ProcessId;
        public int ThreadId;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ProcessBasicInformation
    {
        public IntPtr Reserved1;
        public IntPtr PebBaseAddress;
        public IntPtr Reserved2_0;
        public IntPtr Reserved2_1;
        public IntPtr UniqueProcessId;
        public IntPtr Reserved3;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CreateProcessW(
        string applicationName,
        StringBuilder commandLine,
        IntPtr processAttributes,
        IntPtr threadAttributes,
        bool inheritHandles,
        uint creationFlags,
        IntPtr environment,
        string currentDirectory,
        ref StartupInfo startupInfo,
        out ProcessInformation processInformation);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint ResumeThread(IntPtr thread);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool TerminateProcess(IntPtr process, uint exitCode);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint WaitForSingleObject(IntPtr handle, uint milliseconds);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetExitCodeProcess(IntPtr process, out uint exitCode);

    [DllImport("ntdll.dll")]
    private static extern int NtQueryInformationProcess(
        IntPtr process,
        int processInformationClass,
        out ProcessBasicInformation processInformation,
        int processInformationLength,
        out int returnLength);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(uint access, bool inheritHandle, int processId);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ReadProcessMemory(IntPtr process, IntPtr address, byte[] buffer, int size, out IntPtr bytesRead);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool WriteProcessMemory(IntPtr process, IntPtr address, byte[] buffer, int size, out IntPtr bytesWritten);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool VirtualProtectEx(IntPtr process, IntPtr address, UIntPtr size, uint newProtect, out uint oldProtect);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr handle);

    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            string proxyPath = Process.GetCurrentProcess().MainModule.FileName;
            string directory = Path.GetDirectoryName(proxyPath);
            string proxyBaseName = Path.GetFileNameWithoutExtension(proxyPath);
            string originalPath = Path.Combine(directory, proxyBaseName + ".original.exe");

            if (!File.Exists(originalPath))
                throw new FileNotFoundException("Original Dead Cells executable not found.", originalPath);

            bool disableNews = false;
            bool skipIntro = false;
            bool hideControllerWarning = false;
            bool hideDailyLeaderboard = false;
            bool hideTitleBanner = false;
            var forwarded = new List<string>();
            foreach (string arg in args)
            {
                if (string.Equals(arg, AllSwitch, StringComparison.OrdinalIgnoreCase))
                {
                    disableNews = true;
                    skipIntro = true;
                    hideControllerWarning = true;
                    hideDailyLeaderboard = true;
                    hideTitleBanner = true;
                }
                else if (string.Equals(arg, NoNewsSwitch, StringComparison.OrdinalIgnoreCase))
                    disableNews = true;
                else if (string.Equals(arg, SkipIntroSwitch, StringComparison.OrdinalIgnoreCase))
                    skipIntro = true;
                else if (string.Equals(arg, NoControllerWarningSwitch, StringComparison.OrdinalIgnoreCase))
                    hideControllerWarning = true;
                else if (string.Equals(arg, NoDailyLeaderboardSwitch, StringComparison.OrdinalIgnoreCase))
                    hideDailyLeaderboard = true;
                else if (string.Equals(arg, NoTitleBannerSwitch, StringComparison.OrdinalIgnoreCase))
                    hideTitleBanner = true;
                else
                    forwarded.Add(arg);
            }

            if (skipIntro || hideControllerWarning || hideDailyLeaderboard || hideTitleBanner)
                return RunSuspendedAndPatch(originalPath, directory, forwarded, disableNews,
                    skipIntro, hideControllerWarning, hideDailyLeaderboard, hideTitleBanner);

            var startInfo = new ProcessStartInfo
            {
                FileName = originalPath,
                WorkingDirectory = directory,
                Arguments = JoinArguments(forwarded),
                UseShellExecute = false
            };

            using (Process game = Process.Start(startInfo))
            {
                if (disableNews)
                {
                    byte[] fileBytes = File.ReadAllBytes(originalPath);
                    IntPtr moduleBase = GetModuleBase(game);
                    IntPtr handle = OpenGameProcess(game.Id);
                    try
                    {
                        PatchNewsEndpoint(handle, moduleBase, fileBytes);
                    }
                    finally
                    {
                        CloseHandle(handle);
                    }
                }

                game.WaitForExit();
                return game.ExitCode;
            }
        }
        catch (Exception error)
        {
            MessageBox.Show(
                error.Message,
                "Dead Cells - LaunchTweaks",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return 1;
        }
    }

    private static int RunSuspendedAndPatch(string executablePath, string directory, List<string> forwarded,
        bool disableNews, bool skipIntro, bool hideControllerWarning, bool hideDailyLeaderboard,
        bool hideTitleBanner)
    {
        var startup = new StartupInfo { Size = Marshal.SizeOf(typeof(StartupInfo)) };
        ProcessInformation info;
        string arguments = JoinArguments(forwarded);
        var commandLine = new StringBuilder(QuoteArgument(executablePath));
        if (arguments.Length > 0)
            commandLine.Append(' ').Append(arguments);

        const uint createSuspended = 0x00000004;
        if (!CreateProcessW(executablePath, commandLine, IntPtr.Zero, IntPtr.Zero, false,
            createSuspended, IntPtr.Zero, directory, ref startup, out info))
            ThrowWin32("Could not start the original Dead Cells executable");

        bool resumed = false;
        try
        {
            byte[] fileBytes = File.ReadAllBytes(executablePath);
            IntPtr moduleBase = GetImageBaseFromPeb(info.Process);
            if (skipIntro)
                PatchSkipIntro(info.Process, moduleBase, fileBytes);
            if (hideControllerWarning)
                PatchControllerWarning(info.Process, moduleBase, fileBytes);
            if (hideDailyLeaderboard)
                PatchDailyLeaderboard(info.Process, moduleBase, fileBytes);
            if (hideTitleBanner)
                PatchTitleBanner(info.Process, moduleBase, fileBytes);
            if (disableNews)
                PatchNewsEndpoint(info.Process, moduleBase, fileBytes);

            if (ResumeThread(info.Thread) == uint.MaxValue)
                ThrowWin32("Could not resume Dead Cells after applying SkipIntro");
            resumed = true;

            CloseHandle(info.Thread);
            info.Thread = IntPtr.Zero;

            const uint infinite = 0xFFFFFFFF;
            const uint waitObject0 = 0;
            if (WaitForSingleObject(info.Process, infinite) != waitObject0)
                ThrowWin32("Could not wait for Dead Cells to exit");

            uint exitCode;
            if (!GetExitCodeProcess(info.Process, out exitCode))
                ThrowWin32("Could not read the Dead Cells exit code");
            return unchecked((int)exitCode);
        }
        finally
        {
            if (!resumed && info.Process != IntPtr.Zero)
                TerminateProcess(info.Process, 1);
            if (info.Thread != IntPtr.Zero)
                CloseHandle(info.Thread);
            if (info.Process != IntPtr.Zero)
                CloseHandle(info.Process);
        }
    }

    private static void PatchSkipIntro(IntPtr process, IntPtr moduleBase, byte[] fileBytes)
    {
        int signatureOffset = FindUniquePattern(fileBytes, SkipIntroSignature, "SkipIntro bytecode signature");
        int fileOffset = signatureOffset + SkipIntroBooleanOffset;
        WriteVerifiedBytes(process, moduleBase, fileBytes, fileOffset, new byte[] { 0x00 }, new byte[] { 0x01 }, "SkipIntro");
    }

    private static void PatchNewsEndpoint(IntPtr process, IntPtr moduleBase, byte[] fileBytes)
    {
        byte[] source = Encoding.ASCII.GetBytes(SourceText);
        byte[] replacement = Encoding.ASCII.GetBytes(ReplacementText);
        int fileOffset = FindUniquePattern(fileBytes, source, "news endpoint");
        WriteVerifiedBytes(process, moduleBase, fileBytes, fileOffset, source, replacement, "NoNewsWindow");
    }

    private static void PatchControllerWarning(IntPtr process, IntPtr moduleBase, byte[] fileBytes)
    {
        byte[] source = Encoding.UTF8.GetBytes(ControllerWarningText);
        byte[] replacement = Encoding.ASCII.GetBytes(new string(' ', source.Length));
        int fileOffset = FindUniquePattern(fileBytes, source, "controller recommendation text");
        WriteVerifiedBytes(process, moduleBase, fileBytes, fileOffset, source, replacement, "NoControllerWarning");
    }

    private static void PatchDailyLeaderboard(IntPtr process, IntPtr moduleBase, byte[] fileBytes)
    {
        int fileOffset = FindUniquePattern(fileBytes, DailyLeaderboardVisibilitySignature,
            "daily leaderboard visibility signature");
        WriteVerifiedBytes(process, moduleBase, fileBytes, fileOffset,
            DailyLeaderboardVisibilitySignature, DailyLeaderboardVisibilityReplacement, "NoDailyLeaderboard");
    }

    private static void PatchTitleBanner(IntPtr process, IntPtr moduleBase, byte[] fileBytes)
    {
        int fileOffset = FindUniquePattern(fileBytes, TitleBannerVisibilitySignature,
            "title banner visibility signature");
        WriteVerifiedBytes(process, moduleBase, fileBytes, fileOffset,
            TitleBannerVisibilitySignature, TitleBannerVisibilityReplacement, "NoTitleBanner");
    }

    private static IntPtr GetModuleBase(Process game)
    {
        var timeout = Stopwatch.StartNew();
        IntPtr moduleBase = IntPtr.Zero;
        while (timeout.ElapsedMilliseconds < 5000 && moduleBase == IntPtr.Zero)
        {
            try
            {
                game.Refresh();
                moduleBase = game.MainModule.BaseAddress;
            }
            catch (Win32Exception)
            {
                Thread.Sleep(10);
            }
            catch (InvalidOperationException)
            {
                Thread.Sleep(10);
            }
        }
        if (moduleBase == IntPtr.Zero)
            throw new InvalidOperationException("Could not locate the Dead Cells module in memory.");
        return moduleBase;
    }

    private static IntPtr GetImageBaseFromPeb(IntPtr process)
    {
        ProcessBasicInformation basic;
        int returned;
        int status = NtQueryInformationProcess(process, 0, out basic,
            Marshal.SizeOf(typeof(ProcessBasicInformation)), out returned);
        if (status != 0 || basic.PebBaseAddress == IntPtr.Zero)
            throw new InvalidOperationException("Could not query the suspended Dead Cells process (NTSTATUS 0x" + status.ToString("X8") + ").");

        byte[] imageBaseBytes = new byte[IntPtr.Size];
        IntPtr read;
        IntPtr imageBaseAddress = new IntPtr(basic.PebBaseAddress.ToInt64() + (IntPtr.Size == 8 ? 0x10 : 0x08));
        if (!ReadProcessMemory(process, imageBaseAddress, imageBaseBytes, imageBaseBytes.Length, out read) ||
            read.ToInt64() != imageBaseBytes.Length)
            ThrowWin32("Could not read the Dead Cells image base from its PEB");

        long imageBase = IntPtr.Size == 8 ? BitConverter.ToInt64(imageBaseBytes, 0) : BitConverter.ToInt32(imageBaseBytes, 0);
        if (imageBase == 0)
            throw new InvalidOperationException("The suspended Dead Cells process has no image base.");
        return new IntPtr(imageBase);
    }

    private static IntPtr OpenGameProcess(int processId)
    {
        const uint access = 0x0008 | 0x0010 | 0x0020 | 0x0400;
        IntPtr handle = OpenProcess(access, false, processId);
        if (handle == IntPtr.Zero)
            ThrowWin32("Could not open the Dead Cells process");
        return handle;
    }

    private static void WriteVerifiedBytes(IntPtr process, IntPtr moduleBase, byte[] fileBytes,
        int fileOffset, byte[] expected, byte[] replacement, string patchName)
    {
        if (expected.Length != replacement.Length)
            throw new InvalidOperationException(patchName + " patch has inconsistent byte lengths.");

        long rva = FileOffsetToRva(fileBytes, fileOffset);
        IntPtr address = new IntPtr(moduleBase.ToInt64() + rva);
        byte[] current = new byte[expected.Length];
        IntPtr read;
        if (!ReadProcessMemory(process, address, current, current.Length, out read) || read.ToInt64() != current.Length)
            ThrowWin32("Could not verify Dead Cells memory for " + patchName);
        for (int i = 0; i < expected.Length; i++)
        {
            if (current[i] != expected[i])
                throw new InvalidOperationException("The live game version does not match the expected " + patchName + " bytes.");
        }

        uint oldProtect;
        if (!VirtualProtectEx(process, address, new UIntPtr((uint)replacement.Length), 0x40, out oldProtect))
            ThrowWin32("Could not unlock Dead Cells memory for " + patchName);

        try
        {
            IntPtr written;
            if (!WriteProcessMemory(process, address, replacement, replacement.Length, out written) || written.ToInt64() != replacement.Length)
                ThrowWin32("Could not apply the " + patchName + " patch");
        }
        finally
        {
            uint ignored;
            VirtualProtectEx(process, address, new UIntPtr((uint)replacement.Length), oldProtect, out ignored);
        }

        byte[] verify = new byte[replacement.Length];
        if (!ReadProcessMemory(process, address, verify, verify.Length, out read) || read.ToInt64() != verify.Length)
            throw new InvalidOperationException(patchName + " patch verification failed.");
        for (int i = 0; i < replacement.Length; i++)
        {
            if (verify[i] != replacement[i])
                throw new InvalidOperationException(patchName + " patch verification failed.");
        }
    }

    private static int FindUniquePattern(byte[] bytes, byte[] pattern, string description)
    {
        int found = -1;
        for (int i = 0; i <= bytes.Length - pattern.Length; i++)
        {
            bool match = true;
            for (int j = 0; j < pattern.Length; j++)
            {
                if (bytes[i + j] != pattern[j])
                {
                    match = false;
                    break;
                }
            }
            if (!match)
                continue;
            if (found >= 0)
                throw new InvalidOperationException("Multiple " + description + " matches were found; this game build is not supported.");
            found = i;
            i += pattern.Length - 1;
        }
        if (found < 0)
            throw new InvalidOperationException("The " + description + " was not found; this game build is not supported.");
        return found;
    }

    private static long FileOffsetToRva(byte[] bytes, int fileOffset)
    {
        int peOffset = BitConverter.ToInt32(bytes, 0x3c);
        if (Encoding.ASCII.GetString(bytes, peOffset, 4) != "PE\0\0")
            throw new InvalidOperationException("Invalid PE header in the original game executable.");

        ushort sectionCount = BitConverter.ToUInt16(bytes, peOffset + 6);
        ushort optionalHeaderSize = BitConverter.ToUInt16(bytes, peOffset + 20);
        int sectionTable = peOffset + 24 + optionalHeaderSize;
        for (int i = 0; i < sectionCount; i++)
        {
            int section = sectionTable + i * 40;
            uint virtualAddress = BitConverter.ToUInt32(bytes, section + 12);
            uint rawSize = BitConverter.ToUInt32(bytes, section + 16);
            uint rawOffset = BitConverter.ToUInt32(bytes, section + 20);
            if (fileOffset >= rawOffset && fileOffset < rawOffset + rawSize)
                return virtualAddress + (fileOffset - rawOffset);
        }
        throw new InvalidOperationException("The news endpoint is outside mapped PE sections.");
    }

    private static string JoinArguments(IEnumerable<string> args)
    {
        var result = new StringBuilder();
        foreach (string arg in args)
        {
            if (result.Length > 0)
                result.Append(' ');
            result.Append(QuoteArgument(arg));
        }
        return result.ToString();
    }

    private static string QuoteArgument(string arg)
    {
        if (arg.Length > 0 && arg.IndexOfAny(new[] { ' ', '\t', '\n', '\v', '"' }) < 0)
            return arg;

        var quoted = new StringBuilder("\"");
        int slashes = 0;
        foreach (char ch in arg)
        {
            if (ch == '\\')
            {
                slashes++;
            }
            else if (ch == '"')
            {
                quoted.Append('\\', slashes * 2 + 1);
                quoted.Append('"');
                slashes = 0;
            }
            else
            {
                quoted.Append('\\', slashes);
                quoted.Append(ch);
                slashes = 0;
            }
        }
        quoted.Append('\\', slashes * 2);
        quoted.Append('"');
        return quoted.ToString();
    }

    private static void ThrowWin32(string message)
    {
        throw new Win32Exception(Marshal.GetLastWin32Error(), message);
    }
}
