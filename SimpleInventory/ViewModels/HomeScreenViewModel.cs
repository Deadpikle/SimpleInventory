using Microsoft.Win32;
using SimpleInventory.Helpers;
using SimpleInventory.Interfaces;
using SimpleInventory.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SimpleInventory.ViewModels
{
    class HomeScreenViewModel : BaseViewModel
    {

        public HomeScreenViewModel(IChangeViewModel viewModelChanger) : base(viewModelChanger)
        {
        }

        public ICommand MoveToManageItemsScreen
        {
            get { return new RelayCommand(LoadManageItemsScreen); }
        }

        private void LoadManageItemsScreen()
        {
            PushViewModel(new ManageItemsViewModel(ViewModelChanger) { CurrentUser = CurrentUser });
        }

        public ICommand MoveToScanItemsScreen
        {
            get { return new RelayCommand(LoadScanItemsScreen); }
        }

        private void LoadScanItemsScreen()
        {
            PushViewModel(new ScanItemsViewModel(ViewModelChanger) { CurrentUser = CurrentUser });
        }

        public ICommand MoveToGenerateBarcodesScreen
        {
            get { return new RelayCommand(LoadGenerateBarcodesScreen); }
        }

        private void LoadGenerateBarcodesScreen()
        {
            PushViewModel(new GenerateBarcodesViewModel(ViewModelChanger) { CurrentUser = CurrentUser });
        }

        public ICommand MoveToReportsScreen
        {
            get { return new RelayCommand(LoadReportsScreen); }
        }

        private void LoadReportsScreen()
        {
            PushViewModel(new ViewReportsViewModel(ViewModelChanger) { CurrentUser = CurrentUser });
        }

        public ICommand MoveToManageItemCategoriesScreen
        {
            get { return new RelayCommand(LoadViewItemTypesScreen); }
        }

        private void LoadViewItemTypesScreen()
        {
            PushViewModel(new ViewItemTypesViewModel(ViewModelChanger) { CurrentUser = CurrentUser });
        }

        public ICommand BackupData
        {
            get { return new RelayCommand(BackupDatabase); }
        }

        private void BackupDatabase()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "SIDB file (*.sidb)|*.sidb";
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            saveFileDialog.FileName = "inventory-backup-" + DateTime.Now.ToString("yyyy-MM-dd-H-mm-ss");

            var lastBackupLocation = Properties.Settings.Default.LastBackupFolder;
            if (!string.IsNullOrWhiteSpace(lastBackupLocation) && Directory.Exists(Path.GetDirectoryName(lastBackupLocation)))
            {
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.InitialDirectory = lastBackupLocation;
            }
            if (saveFileDialog.ShowDialog() == true)
            {
                var dbHelper = new DatabaseHelper();
                File.Copy(dbHelper.GetDatabaseFilePath(), saveFileDialog.FileName);
                Properties.Settings.Default.LastBackupFolder = Path.GetDirectoryName(saveFileDialog.FileName);
            }
        }

        public ICommand Logout
        {
            get { return new RelayCommand(PerformLogout); }
        }
        
        private void PerformLogout()
        {
            PopViewModel();
        }

        public ICommand MoveToManageUsersScreen
        {
            get { return new RelayCommand(LoadManageUsersScreen); }
        }

        private void LoadManageUsersScreen()
        {
            PushViewModel(new ManageUsersViewModel(ViewModelChanger) { CurrentUser = CurrentUser });
        }

        public ICommand MoveToChangePasswordScreen
        {
            get { return new RelayCommand(LoadChangePasswordScreen); }
        }

        private void LoadChangePasswordScreen()
        {
            PushViewModel(new ChangePasswordViewModel(ViewModelChanger) { CurrentUser = CurrentUser });
        }
    }
}
