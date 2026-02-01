using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlyNotify.Common;

namespace FlyNotify.Daemon
{
    public class DaemonErrorHandler
    {
        public static List<DaemonError> Errors = new List<DaemonError>();

        /// <summary>
        /// Displays An Error To The User Without Closing FlyNotify
        /// </summary>
        public static void ThrowNonCritical(DaemonError error)
        {
            Errors.Add(error);
            NotificationHelper.Toast("Something Went Wrong", error.Text);
        }

        /// <summary>
        /// Displays An Error To The User And Closes FlyNotify
        /// </summary>
        public static void ThrowCritical(DaemonError error)
        {
            Errors.Add(error);
            NotificationHelper.Toast("Something Went Wrong", error.Text);
            Environment.Exit(1);
        }
    }
}
