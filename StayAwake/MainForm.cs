using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MoveGameProvider
{
    public partial class MainForm : Form
    {
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;
        // Import Windows API
        [DllImport("kernel32.dll")]
        static extern uint SetThreadExecutionState(uint esFlags);

        // Flags
        const uint ES_CONTINUOUS = 0x80000000;
        const uint ES_DISPLAY_REQUIRED = 0x00000002;
        const uint ES_SYSTEM_REQUIRED = 0x00000001;

        public MainForm()
        {
            InitializeComponent();

            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Exit", null, OnExit);

            trayIcon = new NotifyIcon();
            trayIcon.Text = "StayAwake – lid close: do nothing";
            using (var iconStream = typeof(MainForm).Assembly.GetManifestResourceStream("StayAwake.ico")!)
            {
                // Pick the frame that matches the tray size for the current DPI
                trayIcon.Icon = new Icon(iconStream, SystemInformation.SmallIconSize);
            }
            Icon = trayIcon.Icon;
            trayIcon.ContextMenuStrip = trayMenu;
            trayIcon.Visible = true;
        }
        private void OnExit(object? sender, EventArgs e)
        {
            Application.Exit();
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Prevent sleep + display off
            SetThreadExecutionState(
                ES_CONTINUOUS | ES_DISPLAY_REQUIRED | ES_SYSTEM_REQUIRED);

            // Don't sleep (and lock) when the lid is closed
            try
            {
                LidCloseOverride.Apply();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not change the lid-close action:\n" + ex.Message, "StayAwake",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            // Optional: hide the window
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Reset state when app closes (IMPORTANT)
            SetThreadExecutionState(ES_CONTINUOUS);

            try
            {
                LidCloseOverride.Restore();
            }
            catch (Exception)
            {
                // The backup file is kept when restoring fails, so the next launch retries it.
            }

            base.OnFormClosing(e);
        }
    }
}