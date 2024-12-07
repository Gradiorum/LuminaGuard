using System;
using System.Management;

namespace LuminaGuard.Helpers
{
    public class BrightnessController
    {
        public void SetBrightness(byte targetBrightness)
        {
            try
            {
                ManagementScope scope = new ManagementScope("root\\WMI");
                SelectQuery query = new SelectQuery("WmiMonitorBrightnessMethods");
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query))
                {
                    ManagementObjectCollection objectCollection = searcher.Get();
                    foreach (ManagementObject mObj in objectCollection)
                    {
                        mObj.InvokeMethod("WmiSetBrightness", new object[] { uint.MaxValue, targetBrightness });
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Log("Error setting brightness: " + ex.Message);
            }
        }
    }
}
