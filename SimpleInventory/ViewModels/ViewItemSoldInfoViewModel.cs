using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private ObservableCollection<ItemSoldInfo> _itemSoldInfo;
        private DateTime _startDate;
        private DateTime? _endDate;

        private ReportItemSold _reportForItem;
        private User _userToFilterBy;

        public ViewItemSoldInfoViewModel(IChangeViewModel viewModelChanger, DateTime date, ReportItemSold reportForItem, 
            User userToFilterBy = null) : base(viewModelChanger)
        {
            _reportForItem = reportForItem;
            _inventoryItemID = reportForItem.InventoryItemID;
            _item = InventoryItem.LoadItemByID(_inventoryItemID);
            _startDate = date;
            _endDate = null;
            _userToFilterBy = userToFilterBy;
            LoadData();
        }

        public ViewItemSoldInfoViewModel(IChangeViewModel viewModelChanger, DateTime startDate, DateTime endDate, 
            ReportItemSold reportForItem, User userToFilterBy = null) : base(viewModelChanger)
        {
            _reportForItem = reportForItem;
            _inventoryItemID = reportForItem.InventoryItemID;
            _item = InventoryItem.LoadItemByID(_inventoryItemID);
            _startDate = startDate;
            _endDate = endDate;
            _userToFilterBy = userToFilterBy;
            LoadData();
        }

        private void LoadData()
        {
            int userID = _userToFilterBy == null ? -1 : _userToFilterBy.ID;
            if (_endDate != null && _endDate > _startDate && _startDate.Date != _endDate?.Date)
            {
                ItemSoldInfoData = new ObservableCollection<ItemSoldInfo>(ItemSoldInfo.LoadInfoForDateAndItemUntilDate(_startDate, _endDate.Value, 
                    _inventoryItemID, userID));
            }
            else
            {
                ItemSoldInfoData = new ObservableCollection<ItemSoldInfo>(ItemSoldInfo.LoadInfoForDateAndItem(_startDate, _inventoryItemID, userID));
            }
        }

        public IConfirmDelete<ItemSoldInfo> DeleteItemSoldInfoConfirmer { get; set; }
        public IDeletedItemSoldInfo DeletedItemSoldInfoListener { get; set; }

        public ObservableCollection<ItemSoldInfo> ItemSoldInfoData
        {
            get { return _itemSoldInfo; }
            set { _itemSoldInfo = value; NotifyPropertyChanged(); }
        }

        public string ItemNameAndDescription
        {
            get
            {
                string userSoldByName = _userToFilterBy == null ? "" : " (Sold by " + _userToFilterBy.Name + ")";
                if (string.IsNullOrWhiteSpace(_item.Description))
                {
                    return _item.Name + userSoldByName;
                }
                return _item.Name + " - " + _item.Description + userSoldByName;
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
                        _endDate?.ToString(Utilities.DateTimeToFriendlyJustDateStringFormat());
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
        
        public ICommand ConfirmDeleteItemSoldInfo
        {
            get { return new RelayCommand<ItemSoldInfo>(item => CheckBeforeDeletingItemSoldInfo(item)); }
        }

        private void CheckBeforeDeletingItemSoldInfo(ItemSoldInfo item)
        {
            DeleteItemSoldInfoConfirmer?.ConfirmDelete(item);
        }

        public void DeleteItemSoldInfo(ItemSoldInfo info)
        {
            var item = InventoryItem.LoadItemByID(info.InventoryItemID);
            item.AdjustQuantityByAmount(info.QuantitySold);
            info.Delete();
            ItemSoldInfoData.Remove(info);
            ReportForItem = DeletedItemSoldInfoListener?.ItemSoldInfoWasDeleted(info);
            if (ReportForItem == null)
            {
                PopViewModel();
            }
        }
    }
}
