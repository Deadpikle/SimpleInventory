using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SimpleInventory.Helpers;
using SimpleInventory.Interfaces;
using SimpleInventory.Models;

namespace SimpleInventory.ViewModels
{
    class ViewItemSoldInfoViewModel : BaseViewModel
    {
        private int _inventoryItemID;
        private InventoryItem _item;
        private List<ItemSoldInfo> _itemSoldInfo;
        private DateTime _date;

        public ViewItemSoldInfoViewModel(IChangeViewModel viewModelChanger, DateTime date, int inventoryItemID) : base(viewModelChanger)
        {
            _inventoryItemID = inventoryItemID;
            _item = InventoryItem.LoadItemByID(inventoryItemID);
            _itemSoldInfo = ItemSoldInfo.LoadInfoForDateAndItem(date, inventoryItemID);
            _date = date;
        }

        public List<ItemSoldInfo> ItemSoldInfoData
        {
            get { return _itemSoldInfo; }
            set { _itemSoldInfo = value; NotifyPropertyChanged(); }
        }

        public string ItemNameAndDescription
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_item.Description))
                {
                    return _item.Name;
                }
                return _item.Name + " - " + _item.Description;
            }
        }

        public string DateDisplay
        {
            get
            {
                return _date.ToString(Utilities.DateTimeToFriendlyJustDateStringFormat());
            }
        }

        public ICommand ReturnToReports
        {
            get { return new RelayCommand(PopToReports); }
        }

        private void PopToReports()
        {
            PopViewModel();
        }
    }
}
