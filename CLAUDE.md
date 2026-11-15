# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

StayAwake is a small Windows Forms tray app (.NET 10, `net10.0-windows`) that keeps the PC and display awake while it runs. The solution is `StayAwake.slnx` at the repo root, with a single project in `StayAwake/`. There are no tests and no lint configuration.

## Commands

Run these from the repo root:

```
dotnet build StayAwake.slnx
dotnet run --project StayAwake/StayAwake.csproj
dotnet publish StayAwake/StayAwake.csproj -c Release
```

## Architecture

- `Program.cs` starts `MainForm`. All the behavior is in `MainForm.cs`.
- **Keep-awake mechanism:** `OnLoad` P/Invokes `kernel32!SetThreadExecutionState` with `ES_CONTINUOUS | ES_DISPLAY_REQUIRED | ES_SYSTEM_REQUIRED`. `OnFormClosing` resets it with `ES_CONTINUOUS` alone. The flag belongs to the thread that set it, so both calls must stay on the UI thread, which lives as long as the app does. Don't move them to a worker or timer thread.
- **Lid close:** `LidCloseOverride.cs` uses `powrprof.dll` to set the active plan's lid-close action (`SUB_BUTTONS`/`LIDACTION`) to "Do nothing" on startup and restores it on exit. The original values are kept in `%LOCALAPPDATA%\StayAwake\lid-action-backup.txt` until restored, so a crashed run gets fixed on the next launch. On the dev machine (Windows 11, Modern Standby) this setting is hidden in Control Panel, so check it with `powercfg /qh SCHEME_CURRENT SUB_BUTTONS LIDACTION`, not `/q`.
- **UI:** the form starts minimized and is hidden from the taskbar. The user interacts with the app only through a `NotifyIcon`, whose context menu has a single "Exit" item that calls `Application.Exit()`. That call raises `FormClosing`, which runs the reset.
- **Namespace:** the code uses `MoveGameProvider`, not `StayAwake`. Keep new files in that namespace unless you are renaming it everywhere.
- `MainForm.Designer.cs` and `MainForm.resx` belong to the WinForms designer. Put logic in `MainForm.cs`.
- Nullable reference types and implicit usings are enabled.
