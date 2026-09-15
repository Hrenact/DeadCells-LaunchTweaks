# Building from source

Requirements:

- 64-bit Windows
- .NET Framework 4.x, included with supported Windows installations

Open Command Prompt in the repository directory and run:

```bat
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /nologo /target:winexe /platform:x64 /optimize+ /out:DeadCellsLaunchProxy.exe /reference:System.Windows.Forms.dll DeadCellsLaunchProxy.cs
```

The resulting `DeadCellsLaunchProxy.exe` should be placed beside `Install.ps1`.
The project uses only Windows and .NET Framework APIs.
