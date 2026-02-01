#define TRACE // Enable Trace.WriteLine

using System;
using System.Diagnostics;
using System.Drawing;
using Microsoft.Toolkit.Uwp.Notifications;
using FlyNotify.Daemon;
using FlyNotify.Common;
using FlyNotify.GUI;
using IgniteView.Core;
using IgniteView.Desktop;
using System.Reflection;
using System.Runtime.InteropServices;
// using Windows.Services.Store;
using Serilog;
using Serilog.Core;

namespace FlyNotify.Common
{
    public class Program
    {

        public static Daemon.Daemon Background;
        public static AppManager GUI;
        public static IEnumerable<Process> ValidFlyNotifyInstances;
        public static Logger Logger;

        public static bool IsDaemonRunning => ValidFlyNotifyInstances.Where((p) => {
            try
            {
                string commandLine;
                ProcessCommandLine.Retrieve(p, out commandLine, ProcessCommandLine.Parameter.CommandLine);
                return !commandLine.ToLower().Contains("--settings");
            }
            catch { }
            return false;
        }).Any();

        public static bool IsGUIRunning => ValidFlyNotifyInstances.Where((p) => {
            try
            {
                string commandLine;
                ProcessCommandLine.Retrieve(p, out commandLine, ProcessCommandLine.Parameter.CommandLine);
                return commandLine.ToLower().Contains("--settings");
            }
            catch { }
            return false;
        }).Any();

        [STAThread]
        public static void Main(string[] args)
        {
            // Initialize bundle extraction paths for single-file deployment
            var extractionDir = Path.GetDirectoryName(typeof(Program).Assembly.Location);
            if (!string.IsNullOrEmpty(extractionDir))
            {
                var currentPath = Environment.GetEnvironmentVariable("PATH") ?? "";
                var nativePath = Path.Combine(extractionDir, "iv2runtime", "win-x64", "native");
                if (Directory.Exists(nativePath) && !currentPath.Contains(nativePath))
                {
                    Environment.SetEnvironmentVariable("PATH", nativePath + ";" + currentPath);
                }
            }

            AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) =>
            {
                NotificationHelper.MessageBox("Something went wrong with FlyNotify", "Unfortunately, FlyNotify has crashed. Details: " + e.ExceptionObject.ToString());
            };

            //By Default, The App Will Be Launched In Daemon Mode
            //Daemon Mode Is A Background Process That Handles Changing The Position Of Notifications
            //If The "--settings" Arg Is Used, Then The App Will Launch In Settings Mode
            //Settings Mode Shows A GUI That Can Be Used To Configure The App
            //These Mode Switches Ensure All Functions Of The App Use The Same Executable

            //Find Other Instances Of FlyNotify
            ValidFlyNotifyInstances = Process.GetProcessesByName("FlyNotify").Where((p) => {
                try
                {
                    return !p.HasExited && p.Id != Process.GetCurrentProcess().Id;
                }
                catch { }
                return false;
            });

            var isGUIRunning = IsGUIRunning;
            var isDaemonRunning = IsDaemonRunning;

            #if !GUI_DEBUG
            if (!args.Contains("--settings") && isDaemonRunning && !isGUIRunning)
            {
                //Open GUI Instead Of Daemon
                TrayIcon.LaunchSettingsMode(null, null);
                Environment.Exit(1);
            }
            else if (args.Contains("--settings") && isGUIRunning)
            {
                //Exit To Prevent Multiple GUIs
                Environment.Exit(2);
            }
            else if (!args.Contains("--settings") && isDaemonRunning && isGUIRunning)
            {
                //Exit To Prevent Multiple Daemons
                Environment.Exit(3);
            }
            #endif

            DesktopPlatformManager.Activate(); // Needed here to initiate plugin DLL loading

            #if !GUI_DEBUG
            if (args.Contains("--settings"))
            #else
            if (true)
            #endif
            {
                // Initialize Logging For GUI
                Logger = new LoggerConfiguration()
                    .WriteTo.File(Path.Join(Settings.GetAppDataFolder(), "gui.log"), rollingInterval: RollingInterval.Infinite)
                    .CreateLogger();
                Logging.WriteWatermark("GUI");

                // Open The GUI App In Settings Mode
                GUI = new ViteAppManager();
                App();
            }
            else
            {
                // Initialize Logging For Daemon
                Logger = new LoggerConfiguration()
                    .WriteTo.File(Path.Join(Settings.GetAppDataFolder(), "daemon.log"), rollingInterval: RollingInterval.Infinite)
                    .CreateLogger();
                Logging.WriteWatermark("daemon");

                // Open The Background Daemon
                Background = new Daemon.Daemon();
            }

            // Sync startup registration
            try
            {
                var settings = Settings.Get();
                Util.SetStartup(settings.RunOnStartup);
            }
            catch { }

        }

        public static async Task App()
        {
            // Copy The Wallpaper File So That The GUI Can Access It
            WallpaperFinder.CopyWallpaper();
            AppManager.Instance.RegisterDynamicFileRoute("/wallpaper.jpg", WallpaperFinder.WallpaperRoute);

            var mainWindow =
                WebWindow.Create()
                .WithTitle("FlyNotify")
                .WithBounds(new LockedWindowBounds((int)(400f * ResolutionFinder.GetScale()), (int)(650f * ResolutionFinder.GetScale())))
                .With((w) => (w as Win32WebWindow).BackgroundMode = Win32WebWindow.WindowBackgroundMode.Acrylic)
                .WithoutTitleBar()
                .Show();



            // Clean Up
            GUI.OnCleanUp += () =>
            {
                WallpaperFinder.CleanUp();
                ToastNotificationManagerCompat.Uninstall();
            };

            GUI.Run();
        }

    }
}

