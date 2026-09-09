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
            trayIcon.Text = "State Server";
            trayIcon.Icon = SystemIcons.WinLogo;
            trayIcon.ContextMenuStrip = trayMenu;
            trayIcon.Visible = true;
        }
        private void OnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Prevent sleep + display off
            SetThreadExecutionState(
                ES_CONTINUOUS | ES_DISPLAY_REQUIRED | ES_SYSTEM_REQUIRED);

            // Optional: hide the window
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Reset state when app closes (IMPORTANT)
            SetThreadExecutionState(ES_CONTINUOUS);

            base.OnFormClosing(e);
        }
    }
}