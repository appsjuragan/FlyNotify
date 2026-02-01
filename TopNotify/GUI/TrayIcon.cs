using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FlyNotify.Common;
using FlyNotify.Daemon;

namespace FlyNotify.GUI
{
    public class TrayIcon
    {
        private static NotifyIcon _notifyIcon;
        private static ContextMenuStrip _contextMenu;

        public static void Setup()
        {
            _contextMenu = new ContextMenuStrip();
            _contextMenu.Items.Add("Create Bug Report", null, (s, e) => BugReport.DisplayBugReport(BugReport.CreateBugReport()));
            _contextMenu.Items.Add("Quit FlyNotify", null, (s, e) => Quit());

            _notifyIcon = new NotifyIcon
            {
                Icon = Util.FindAppIcon(),
                Text = "FlyNotify",
                Visible = true,
                ContextMenuStrip = _contextMenu
            };

            _notifyIcon.DoubleClick += (s, e) => LaunchSettingsMode(s, e);
        }

        public static void MainLoop()
        {
            Application.Run();
        }

        public static void Quit()
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
            }
            
            //Kill Other Instances
            var instances = Process.GetProcessesByName("FlyNotify");
            foreach (var instance in instances)
            {
                if (instance.Id != Process.GetCurrentProcess().Id)
                {
                    try
                    {
                        instance.Kill();
                    }
                    catch { }
                }
            }

            Environment.Exit(0);
        }

        public static void LaunchSettingsMode(object? sender, EventArgs e)
        {
            try
            {
                var exe = Util.FindExe();
                var psi = new ProcessStartInfo(exe, "--settings" + (Debugger.IsAttached ? " --debug-process" : ""));
                psi.UseShellExecute = false;
                psi.WorkingDirectory = Util.BaseDir;
                Process.Start(psi);
            }
            catch (Exception)
            {
                // Ignore
            }
        }
    }
}
