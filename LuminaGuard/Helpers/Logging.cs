using System;
using System.IO;
using System.Threading;

namespace LuminaGuard.Helpers
{
    public static class Logging
    {
        private static readonly object lockObj = new object();

        public static void Log(string message)
        {
            try
            {
                lock (lockObj)
                {
                    string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");
                    File.AppendAllText(logPath, DateTime.Now + ": " + message + Environment.NewLine);
                }
            }
            catch { }
        }
    }
}
