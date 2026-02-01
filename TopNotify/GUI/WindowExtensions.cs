using IgniteView.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlyNotify.Common;

namespace FlyNotify.GUI
{
    public static class WindowExtensions
    {
        public static void SendConfig(this WebWindow target)
        {
            var config = Settings.GetForIPC();
            target.CallFunction("SetConfig", config);
        }
    }
}
