using System;

namespace SimpleInventory.Enums
{
    public enum PurchaseMethod
    {
        Cash = 1,
        QRCode = 2
    }

    // https://stackoverflow.com/a/5985710
    static class PurchaseMethodMethods
    {
        public static string ToHumanReadableString(this PurchaseMethod pm)
        {
            switch (pm)
            {
                case PurchaseMethod.Cash:
                    return "Cash";
                case PurchaseMethod.QRCode:
                    return "QR Code";
                default:
                    return "Cash";
            }
        }
    }
}
