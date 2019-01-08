using SimpleInventory.Models;
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
            return "yyyy-MM-dd H:mm:ss"; // 24 hr time (0-23)
        }

        public static string DateTimeToDateOnlyStringFormat()
        {
            return "yyyy-MM-dd"; // 24 hr time (0-23)
        }

        public static decimal ConvertAmount(decimal amount, Currency initialCurrency, Currency toCurrency)
        {
            if (initialCurrency.ConversionRateToUSD == 1)
            {
                return amount * toCurrency.ConversionRateToUSD;
            }
            else
            {
                // / = convert to USD, then convert to other currency
                return amount / initialCurrency.ConversionRateToUSD * toCurrency.ConversionRateToUSD;
            }
        }
    }
}
