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
        private DateTime _startDate;
        private DateTime _endDate;

        public ViewItemSoldInfoViewModel(IChangeViewModel viewModelChanger, DateTime date, int inventoryItemID) : base(viewModelChanger)
        {
            _inventoryItemID = inventoryItemID;
            _item = InventoryItem.LoadItemByID(inventoryItemID);
            _itemSoldInfo = ItemSoldInfo.LoadInfoForDateAndItem(date, inventoryItemID);
            _startDate = date;
        }

        public ViewItemSoldInfoViewModel(IChangeViewModel viewModelChanger, DateTime startDate, DateTime endDate, int inventoryItemID) : base(viewModelChanger)
        {
            _inventoryItemID = inventoryItemID;
            _item = InventoryItem.LoadItemByID(inventoryItemID);
            _itemSoldInfo = ItemSoldInfo.LoadInfoForDateAndItem(startDate, inventoryItemID);
            // THE UNOPTIMIZATION OF THE MODEL CODE BURNS
            if (endDate > startDate && startDate.Date != endDate.Date)
            {
                bool isFinished = false;
                var movingDate = startDate;
                while (!isFinished)
                {
                    if (movingDate.Date == endDate.Date)
                    {
                        isFinished = true;
                    }
                    else
                    {
                        movingDate = movingDate.AddDays(1);
                        _itemSoldInfo.AddRange(ItemSoldInfo.LoadInfoForDateAndItem(movingDate, inventoryItemID));
                    }
                }
            }
            _startDate = startDate;
            _endDate = endDate;
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
                if (_endDate == null)
                {
                    return _startDate.ToString(Utilities.DateTimeToFriendlyJustDateStringFormat());
                }
                else
                {
                    return _startDate.ToString(Utilities.DateTimeToFriendlyJustDateStringFormat()) + " - " +
                        _endDate.ToString(Utilities.DateTimeToFriendlyJustDateStringFormat());
                }
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
