using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Left_Side_Alerts
{
    public static class Logger
    {
        [Conditional("DEBUG")]
        public static void Message(string message)
        {
            Log.Message("[Left Side Alerts] " + message);
        }

        [Conditional("DEBUG")]
        public static void Warning(string message)
        {
            Log.Warning("[Left Side Alerts] " + message);
        }

        [Conditional("DEBUG")]
        public static void Error(string message)
        {
            Log.Error("[Left Side Alerts] " + message);
        }

        [Conditional("DEBUG")]
        public static void Exception(Exception exception, string context = null)
        {
            if (exception == null)
            {
                return;
            }

            var prefix = string.IsNullOrWhiteSpace(context) ? "[Left Side Alerts] " : "[Left Side Alerts] " + context + ": ";
            Log.Error(prefix + exception);
        }
    }
}
