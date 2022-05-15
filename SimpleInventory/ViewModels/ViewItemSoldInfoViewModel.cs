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
        private ObservableCollection<IItemSoldInfo> _itemSoldInfo;
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
                var itemSoldList = new List<IItemSoldInfo>();
                itemSoldList.AddRange(ItemSoldInfo.LoadInfoForDateAndItemUntilDate(_startDate, _endDate.Value,
                    _inventoryItemID, userID));
                var purchases = Purchase.LoadInfoForDateAndItemUntilDate(_startDate, _endDate.Value,
                    _inventoryItemID, userID);
                foreach (var purchase in purchases)
                {
                    itemSoldList.AddRange(purchase.Items);
                }
                ItemSoldInfoData = new ObservableCollection<IItemSoldInfo>(itemSoldList);
                
            }
            else
            {
                var itemSoldList = new List<IItemSoldInfo>();
                itemSoldList.AddRange(ItemSoldInfo.LoadInfoForDateAndItem(_startDate, _inventoryItemID, userID));
                var purchases = Purchase.LoadInfoForDateAndItem(_startDate, _inventoryItemID, userID);
                foreach (var purchase in purchases)
                {
                    itemSoldList.AddRange(purchase.Items);
                }
                ItemSoldInfoData = new ObservableCollection<IItemSoldInfo>(itemSoldList);
            }
        }

        public IConfirmDelete<IItemSoldInfo> DeleteItemSoldInfoConfirmer { get; set; }
        public IDeletedItemSoldInfo DeletedItemSoldInfoListener { get; set; }

        public ObservableCollection<IItemSoldInfo> ItemSoldInfoData
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
            get { return new RelayCommand<IItemSoldInfo>(item => CheckBeforeDeletingItemSoldInfo(item)); }
        }

        private void CheckBeforeDeletingItemSoldInfo(IItemSoldInfo item)
        {
            DeleteItemSoldInfoConfirmer?.ConfirmDelete(item);
        }

        public void DeleteItemSoldInfo(IItemSoldInfo info)
        {
            if (info is ItemSoldInfo isi)
            {
                var item = InventoryItem.LoadItemByID(isi.InventoryItemID);
                item.AdjustQuantityByAmount(isi.QuantitySold);
                isi.Delete();
                ItemSoldInfoData.Remove(isi);
                ReportForItem = DeletedItemSoldInfoListener?.ItemSoldInfoWasDeleted(isi);
                if (ReportForItem == null)
                {
                    PopViewModel();
                }
            }
            else if (info is PurchasedItem pitem)
            {
                var item = InventoryItem.LoadItemByID(pitem.InventoryItemID);
                item.AdjustQuantityByAmount(pitem.QuantitySold);
                pitem.Delete();
                var purchase = Purchase.LoadPurchaseByID(pitem.PurchaseID);
                if (purchase != null && purchase.Items.Count == 0)
                {
                    // nothing left.
                    purchase.Delete();
                }
                ItemSoldInfoData.Remove(pitem);
                ReportForItem = DeletedItemSoldInfoListener?.ItemSoldInfoWasDeleted(pitem);
                if (ReportForItem == null)
                {
                    PopViewModel();
                }
            }
        }
    }
}
