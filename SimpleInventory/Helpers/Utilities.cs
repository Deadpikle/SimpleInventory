using SimpleInventory.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
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
            return "yyyy-MM-dd HH:mm:ss"; // 24 hr time (0-23)
        }

        public static string DateTimeToDateOnlyStringFormat()
        {
            return "yyyy-MM-dd";
        }

        public static string DateTimeToFriendlyFullDateTimeStringFormat()
        {
            return "dddd, d MMMM, yyyy 'at' h:mm:ss tt";
        }

        public static string DateTimeToFriendlyJustDateStringFormat()
        {
            return "dddd, d MMMM, yyyy";
        }

        public static decimal ConvertAmount(decimal amount, Currency initialCurrency, Currency toCurrency)
        {
            if (initialCurrency.ID == toCurrency.ID)
            {
                return amount;
            }
            else
            {
                // / = convert to USD, then convert to other currency
                return amount / initialCurrency.ConversionRateToUSD * toCurrency.ConversionRateToUSD;
            }
        }

        public static decimal ConvertAmountWithRates(decimal amount, decimal initialCurrencyConversion, decimal toCurrencyConversion)
        {
            // / = convert to USD, then convert to other currency
            return amount / initialCurrencyConversion * toCurrencyConversion;
        }

        public static Currency CurrencyForOrder(List<ItemSoldInfo> items)
        {
            Currency currency = null;
            foreach (var item in items)
            {
                if (currency == null)
                {
                    currency = item.CostCurrency;
                }
                else if (item.CostCurrency != null)
                {
                    if (currency.ID != item.CostCurrency.ID)
                    {
                        return null; // not all currencies are the same
                    }
                }
            }
            return currency;
        }

        // https://stackoverflow.com/a/819705
        // I don't care _that_ much about this string being in RAM for a short time. :)
        public static string SecureStringToString(SecureString value)
        {
            IntPtr valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(value);
                return Marshal.PtrToStringUni(valuePtr);
            }
            catch { }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
            return "";
        }
    }
}
