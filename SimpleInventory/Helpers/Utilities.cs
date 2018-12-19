using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleInventory.Helpers
{
    class Utilities
    {
        public static bool InDesignMode()
        {
            return  LicenseManager.UsageMode == LicenseUsageMode.Designtime;
        }

        public static string DateTimeToStringFormat()
        {
            return "yyyy-m-d H:m:s"; // 24 hr time (0-23)
        }
    }
}
