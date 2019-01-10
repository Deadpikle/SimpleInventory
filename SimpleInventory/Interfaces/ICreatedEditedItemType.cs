using SimpleInventory.Models;

namespace SimpleInventory.Interfaces
{
    interface ICreatedEditedItemType
    {
        void CreatedEditedItemType(ItemType itemType, bool wasCreated);
    }
}
