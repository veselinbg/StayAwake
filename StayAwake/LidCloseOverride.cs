using System.ComponentModel;
using System.Runtime.InteropServices;

namespace MoveGameProvider
{
    /// <summary>
    /// While the app runs, sets the active power plan's "When I close the lid" action to
    /// "Do nothing" (plugged in and on battery), so closing the lid doesn't sleep - and
    /// therefore lock - the laptop. The original values are restored on exit.
    /// </summary>
    internal static class LidCloseOverride
    {
        static Guid SubgroupButtons = new("4f971e89-eebd-4455-a8de-9e59040e7347"); // GUID_SYSTEM_BUTTON_SUBGROUP
        static Guid LidAction = new("5ca83367-6e45-459f-a27b-476b1d01c936");       // GUID_LIDCLOSE_ACTION
        const uint DoNothing = 0;

        // The original values are kept on disk until restored, so if the app crashes or is
        // killed, the next launch puts the user's real setting back before overriding it again.
        static readonly string BackupPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "StayAwake", "lid-action-backup.txt");

        public static void Apply()
        {
            Restore();

            Guid scheme = GetActiveScheme();
            Check(PowerReadACValueIndex(IntPtr.Zero, ref scheme, ref SubgroupButtons, ref LidAction, out uint ac));
            Check(PowerReadDCValueIndex(IntPtr.Zero, ref scheme, ref SubgroupButtons, ref LidAction, out uint dc));

            Directory.CreateDirectory(Path.GetDirectoryName(BackupPath)!);
            File.WriteAllLines(BackupPath, [scheme.ToString(), ac.ToString(), dc.ToString()]);

            Write(scheme, DoNothing, DoNothing);
        }

        public static void Restore()
        {
            if (!File.Exists(BackupPath))
                return;

            string[] lines = File.ReadAllLines(BackupPath);
            Write(Guid.Parse(lines[0]), uint.Parse(lines[1]), uint.Parse(lines[2]));
            File.Delete(BackupPath);
        }

        static void Write(Guid scheme, uint ac, uint dc)
        {
            Check(PowerWriteACValueIndex(IntPtr.Zero, ref scheme, ref SubgroupButtons, ref LidAction, ac));
            Check(PowerWriteDCValueIndex(IntPtr.Zero, ref scheme, ref SubgroupButtons, ref LidAction, dc));

            // Changes to the active plan only take effect once it is re-applied.
            if (GetActiveScheme() == scheme)
                Check(PowerSetActiveScheme(IntPtr.Zero, ref scheme));
        }

        static Guid GetActiveScheme()
        {
            Check(PowerGetActiveScheme(IntPtr.Zero, out IntPtr ptr));
            try { return Marshal.PtrToStructure<Guid>(ptr); }
            finally { LocalFree(ptr); }
        }

        static void Check(uint error)
        {
            if (error != 0)
                throw new Win32Exception((int)error);
        }

        [DllImport("powrprof.dll")]
        static extern uint PowerGetActiveScheme(IntPtr userRootPowerKey, out IntPtr activePolicyGuid);

        [DllImport("powrprof.dll")]
        static extern uint PowerSetActiveScheme(IntPtr userRootPowerKey, ref Guid schemeGuid);

        [DllImport("powrprof.dll")]
        static extern uint PowerReadACValueIndex(IntPtr rootPowerKey, ref Guid schemeGuid, ref Guid subGroupGuid, ref Guid settingGuid, out uint value);

        [DllImport("powrprof.dll")]
        static extern uint PowerReadDCValueIndex(IntPtr rootPowerKey, ref Guid schemeGuid, ref Guid subGroupGuid, ref Guid settingGuid, out uint value);

        [DllImport("powrprof.dll")]
        static extern uint PowerWriteACValueIndex(IntPtr rootPowerKey, ref Guid schemeGuid, ref Guid subGroupGuid, ref Guid settingGuid, uint value);

        [DllImport("powrprof.dll")]
        static extern uint PowerWriteDCValueIndex(IntPtr rootPowerKey, ref Guid schemeGuid, ref Guid subGroupGuid, ref Guid settingGuid, uint value);

        [DllImport("kernel32.dll")]
        static extern IntPtr LocalFree(IntPtr mem);
    }
}
