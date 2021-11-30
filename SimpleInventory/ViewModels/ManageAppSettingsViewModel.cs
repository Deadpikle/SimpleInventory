using SimpleInventory.Helpers;
using SimpleInventory.Interfaces;
using SimpleInventory.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SimpleInventory.ViewModels
{
    class ManageAppSettingsViewModel : BaseViewModel
    {
        private uint _autoLogoutLengthMinutes;

        public ManageAppSettingsViewModel(IChangeViewModel viewModelChanger) : base(viewModelChanger)
        {
            _autoLogoutLengthMinutes = Properties.Settings.Default.AutoLogoutLength;
        }

        #region Properties

        public uint AutoLogoutLengthMinutes
        {
            get { return _autoLogoutLengthMinutes; }
            set { _autoLogoutLengthMinutes = value; NotifyPropertyChanged(); }
        }

        #endregion

        public ICommand ReturnToMainMenu
        {
            get { return new RelayCommand(PopToMainMenu); }
        }

        private void PopToMainMenu()
        {
            PopViewModel();
        }

        public ICommand SaveAppSettings
        {
            get { return new RelayCommand(SaveAndPop); }
        }

        private void SaveAndPop()
        {
            // save
            Properties.Settings.Default.AutoLogoutLength = AutoLogoutLengthMinutes;
            // pop
            PopToMainMenu();
        }
    }
}
