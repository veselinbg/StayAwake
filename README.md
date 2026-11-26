# StayAwake

A small Windows tray app (.NET 10) that keeps the PC and the display awake while it runs, even with the laptop lid closed.

## How it works

- **No sleep, no display off**: on start it calls `SetThreadExecutionState` (kernel32) with `ES_CONTINUOUS | ES_DISPLAY_REQUIRED | ES_SYSTEM_REQUIRED`, and resets it on exit.
- **Lid close**: it sets the active power plan's lid-close action to "Do nothing" through `powrprof.dll` and restores the original value on exit.
- **Crash-safe**: the original lid-close values are saved in `%LOCALAPPDATA%\StayAwake\lid-action-backup.txt` until restored, so a crashed run is fixed on the next start.
- **Tray only**: no window and no taskbar button. Right-click the tray icon and choose **Exit** to stop and restore everything.

## Build and run

Requires the .NET 10 SDK on Windows.

```
dotnet build StayAwake.slnx
dotnet run --project StayAwake/StayAwake.csproj
dotnet publish StayAwake/StayAwake.csproj -c Release
```

Tip: on Windows 11 with Modern Standby the lid setting may be hidden in Control Panel. Check it with:

```
powercfg /qh SCHEME_CURRENT SUB_BUTTONS LIDACTION
```

## Development

Developed with Claude Code. [CLAUDE.md](CLAUDE.md) describes the architecture and the rules the agent follows, for example why the keep-awake calls must stay on the UI thread.

## Tech stack

C# · .NET 10 · WinForms · Win32 P/Invoke (kernel32, powrprof)
