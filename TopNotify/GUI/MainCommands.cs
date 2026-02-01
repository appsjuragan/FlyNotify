using IgniteView.Core;
using IgniteView.Desktop;
using Newtonsoft.Json;
using SamsidParty_TopNotify.Daemon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using TopNotify.Common;
using TopNotify.Daemon;
// using Windows.ApplicationModel.Store;

namespace TopNotify.GUI
{
    public class MainCommands
    {
        static bool isSaving = false;

        //Called By JavaScript
        //Spawns A Test Notification
        [Command("SpawnTestNotification")]
        public static void SpawnTestNotification()
        {
            NotificationTester.SpawnTestNotification();
        }

        //Called By JavaScript
        //Opens The About Page
        [Command("About")]
        public static void About()
        {
            WebWindow.Create("/index.html?about")
                .WithTitle("About FlyNotify")
                .WithBounds(new LockedWindowBounds((int)(400f * ResolutionFinder.GetScale()), (int)(300f * ResolutionFinder.GetScale())))
                .With((w) => (w as Win32WebWindow).BackgroundMode = Win32WebWindow.WindowBackgroundMode.Acrylic)
                .WithoutTitleBar()
                .Show();
        }

        [Command("Donate")] 
        public static void Donate()
        {
             NotificationTester.MessageBox("Donation", "Store donations are not available in the portable version. Please visit our GitHub or website.");
        }

        [Command("GetVersion")]
        public static string GetVersion()
        {
            try
            {
                var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                return version != null ? $" v{version.Major}.{version.Minor}.{version.Build}" : " v?.?.?";
            }
            catch
            {
                return " Portable";
            }
        }

        //Called By JavaScript
        //Sends The Config File
        [Command("RequestConfig")]
        public static void RequestConfig(WebWindow target)
        {
            target.SendConfig();
        }

        //Called By JavaScript
        //Write Settings File
        [Command("WriteConfigFile")]
        public static void WriteConfigFile(WebWindow target, string data)
        {

            if (isSaving) { return; }
            isSaving = true;

            Settings.Overwrite(data);

            Thread.Sleep(100); // Prevent Crashing Daemon From Spamming Button

            // Tell The Daemon The Config Has Changed
            Daemon.Daemon.SendCommandToDaemon("UpdateConfig");

            isSaving = false;
        }

        [Command("OpenAppFolder")]
        public static async Task OpenAppFolder(WebWindow target)
        {
            Process.Start("explorer.exe", Settings.GetAppDataFolder());
        }

        [Command("OpenSoundFolder")]
        public static async Task OpenSoundFolder(WebWindow target)
        {
            Process.Start("explorer.exe", SoundFinder.ImportedSoundFolder);
        }
    }
}
