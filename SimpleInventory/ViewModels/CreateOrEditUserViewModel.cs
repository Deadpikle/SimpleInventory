using SimpleInventory.Helpers;
using SimpleInventory.Interfaces;
using SimpleInventory.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SimpleInventory.ViewModels
{
    class CreateOrEditUserViewModel : BaseViewModel
    {
        private bool _isCreating;
        private User _user;

        private string _screenTitle;

        public CreateOrEditUserViewModel(IChangeViewModel viewModelChanger) : base(viewModelChanger)
        {
            _isCreating = true;
            _user = new User();
            _user.Permissions = new UserPermissions();
            ScreenTitle = "Add User";
        }

        public CreateOrEditUserViewModel(IChangeViewModel viewModelChanger, User userToEdit) : base(viewModelChanger)
        {
            _isCreating = false;
            _user = userToEdit;
            ScreenTitle = "Edit User";
        }

        public User User
        {
            get { return _user; }
            set { _user = value; NotifyPropertyChanged(); }
        }

        public string ScreenTitle
        {
            get { return _screenTitle; }
            set { _screenTitle = value; NotifyPropertyChanged(); }
        }

        public ICommand ReturnToManageUsers
        {
            get { return new RelayCommand(GoBackToManageUsers); }
        }

        private void GoBackToManageUsers()
        {
            PopViewModel();
        }

        public ICommand SaveUser
        {
            get { return new RelayCommand(SaveAndGoBack); }
        }

        private void SaveAndGoBack()
        {
            if (_isCreating)
            {
                // create new
            }
            else
            {
                // save updates
            }
        }
    }
}
