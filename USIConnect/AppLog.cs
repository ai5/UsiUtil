using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace USIConnect
{
    public static class AppLog
    {
        private static SynchronizationContext syncContext; // 同期用

        public static event AppLogEventHandler ErrorEvent;

        public static event AppLogEventHandler InfoEvent;

        public static void Initialize()
        {
            syncContext = SynchronizationContext.Current;
            Debug.Assert(syncContext != null, "カレントに値が設定されていること");
        }

        /// <summary>
        /// Info
        /// </summary>
        /// <param name="message"></param>
        public static void Info(string message)
        {
            syncContext.Post(
                s =>
                {
                    if (InfoEvent != null)
                    {
                        InfoEvent(null, new AppLogEventArgs(message));
                    }
                },
                null);
        }

        /// <summary>
        /// Info
        /// </summary>
        /// <param name="message"></param>
        public static void Error(string message)
        {
            syncContext.Post(
                s =>
                {
                    if (ErrorEvent != null)
                    {
                        ErrorEvent(null, new AppLogEventArgs(message));
                    }
                },
                null);
        }
    }

    public delegate void AppLogEventHandler(object sender, AppLogEventArgs e);

    public class AppLogEventArgs
    {
        public string Message;

        public AppLogEventArgs(string message)
        {
            this.Message = message;
        }
    }
}
