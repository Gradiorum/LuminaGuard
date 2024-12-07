using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;

namespace LuminaGuard.Helpers
{
    public class ProcessMonitor
    {
        public bool AnyProcessRunning(IEnumerable<string> processNames)
        {
            var lowerNames = processNames.Select(n => n.ToLower()).ToList();
            var runningProcesses = Process.GetProcesses();
            foreach (var p in runningProcesses)
            {
                try
                {
                    if (lowerNames.Contains(p.ProcessName.ToLower()))
                    {
                        return true;
                    }
                }
                catch { }
            }
            return false;
        }
    }
}
