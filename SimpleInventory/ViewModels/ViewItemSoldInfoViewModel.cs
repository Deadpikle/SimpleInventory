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

        private ReportItemSold _reportForItem;

        public ViewItemSoldInfoViewModel(IChangeViewModel viewModelChanger, DateTime date, ReportItemSold reportForItem) : base(viewModelChanger)
        {
            _reportForItem = reportForItem;
            _inventoryItemID = reportForItem.InventoryItemID;
            _item = InventoryItem.LoadItemByID(_inventoryItemID);
            _itemSoldInfo = ItemSoldInfo.LoadInfoForDateAndItem(date, _inventoryItemID);
            _startDate = date;
        }

        public ViewItemSoldInfoViewModel(IChangeViewModel viewModelChanger, DateTime startDate, DateTime endDate, ReportItemSold reportForItem) : base(viewModelChanger)
        {
            _reportForItem = reportForItem;
            _inventoryItemID = reportForItem.InventoryItemID;
            _item = InventoryItem.LoadItemByID(_inventoryItemID);
            if (endDate != null && endDate > startDate && startDate.Date != endDate.Date)
            {
                _itemSoldInfo = ItemSoldInfo.LoadInfoForDateAndItemUntilDate(startDate, endDate, _inventoryItemID);
            }
            else
            {
                _itemSoldInfo = ItemSoldInfo.LoadInfoForDateAndItem(startDate, _inventoryItemID);
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

        public ReportItemSold ReportForItem
        {
            get { return _reportForItem; }
            set { _reportForItem = value; NotifyPropertyChanged(); }
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
