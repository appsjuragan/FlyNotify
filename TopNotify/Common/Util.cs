using IgniteView.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FlyNotify.Common
{
    public class Util
    {
        private static string _baseDir = null;
        public static string BaseDir
        {
            get
            {
                if (_baseDir != null) return _baseDir;

                // Priority 1: Check where the assembly is (important for single-file extraction)
                var assemblyDir = Path.GetDirectoryName(typeof(Util).Assembly.Location);
                if (!string.IsNullOrEmpty(assemblyDir) && Directory.Exists(Path.Combine(assemblyDir, "iv2runtime")))
                {
                    return _baseDir = assemblyDir;
                }

                // Priority 2: Use AppDomain BaseDirectory
                var appDomainDir = AppDomain.CurrentDomain.BaseDirectory;
                if (Directory.Exists(Path.Combine(appDomainDir, "iv2runtime")))
                {
                    return _baseDir = appDomainDir;
                }

                // Fallback
                return _baseDir = appDomainDir;
            }
        }

        /// <summary>
        /// Runs A Command Prompt Command And Returns The Output
        /// </summary>
        /// <param name="cmdString"></param>
        /// <returns></returns>
        public static string SimpleCMD(string cmdString)
        {
            var command = "/c " + cmdString;
            var cmdsi = new ProcessStartInfo("cmd.exe");
            cmdsi.Arguments = command;
            cmdsi.RedirectStandardOutput = true;
            cmdsi.UseShellExecute = false;
            cmdsi.CreateNoWindow = true;
            var cmd = Process.Start(cmdsi);
            var output = cmd.StandardOutput.ReadToEnd();

            cmd.WaitForExit();

            output = (new Regex("[ ]{2,}", RegexOptions.None)).Replace(output, " "); //Remove Double Spaces
            return output;
        }

        public static Icon FindAppIcon()
        {
            var path = Path.Combine(BaseDir, "iv2runtime", "Icon.ico");
            return new Icon(path);
        }

        public static FileResolver GetFileResolver()
        {
            if (AppManager.Instance?.CurrentServerManager?.Resolver != null) 
            { 
                return AppManager.Instance.CurrentServerManager.Resolver;
            }

            return new TarFileResolver();
        }

        public static string FindExe()
        {
            return Process.GetCurrentProcess().MainModule.FileName;
        }
    }
}
