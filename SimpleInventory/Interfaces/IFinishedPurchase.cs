using SimpleInventory.Models;

namespace SimpleInventory.Interfaces
{
    interface IFinishedPurchase
    {
        void FinishedPurchase(Purchase purchase);
    }
}
