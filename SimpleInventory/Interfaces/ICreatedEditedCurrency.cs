using SimpleInventory.Models;

namespace SimpleInventory.Interfaces
{
    interface ICreatedEditedCurrency
    {
        void CreatedEditedCurrency(Currency currency, bool wasCreated);
    }
}
