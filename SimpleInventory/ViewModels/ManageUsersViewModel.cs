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
    class ManageUsersViewModel : BaseViewModel
    {
        private List<User> _users;
        private User _selectedUser;

        public ManageUsersViewModel(IChangeViewModel viewModelChanger) : base(viewModelChanger)
        {
            _users = User.LoadUsers();
        }

        public List<User> Users
        {
            get { return _users; }
            set { _users = value; NotifyPropertyChanged(); }
        }

        public User SelectedUser
        {
            get { return _selectedUser; }
            set { _selectedUser = value; NotifyPropertyChanged(); }
        }

        public bool CanEditDeleteCurrentUser
        {
            get { return _selectedUser != null && _selectedUser.ID == CurrentUser.ID; }
        }

        public ICommand GoToMainMenu
        {
            get { return new RelayCommand(PopToMainMenu); }
        }

        private void PopToMainMenu()
        {
            PopViewModel();
        }

        public ICommand MoveToAddUserScreen
        {
            get { return new RelayCommand(LoadAddUserScreen); }
        }

        public ICommand MoveToEditUserScreen
        {
            get { return new RelayCommand(LoadEditUserScreen); }
        }

        private void LoadAddUserScreen()
        {
            PushViewModel(new CreateOrEditUserViewModel(ViewModelChanger) { CurrentUser = CurrentUser });
        }

        private void LoadEditUserScreen()
        {
            PushViewModel(new CreateOrEditUserViewModel(ViewModelChanger, SelectedUser) { CurrentUser = CurrentUser });
        }
    }
}
