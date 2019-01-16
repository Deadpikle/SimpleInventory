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
    class ManageUsersViewModel : BaseViewModel, ICreatedUser
    {
        private ObservableCollection<User> _users;
        private User _selectedUser;

        public ManageUsersViewModel(IChangeViewModel viewModelChanger) : base(viewModelChanger)
        {
            _users = new ObservableCollection<User>(User.LoadUsers());
        }

        public ObservableCollection<User> Users
        {
            get { return _users; }
            set { _users = value; NotifyPropertyChanged(); }
        }

        public User SelectedUser
        {
            get { return _selectedUser; }
            set { _selectedUser = value; NotifyPropertyChanged(); NotifyPropertyChanged(nameof(CanEditDeleteCurrentUser)); }
        }

        public bool CanEditDeleteCurrentUser
        {
            get { return _selectedUser != null && _selectedUser.ID != CurrentUser.ID; }
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
            PushViewModel(new CreateOrEditUserViewModel(ViewModelChanger, this) { CurrentUser = CurrentUser });
        }

        private void LoadEditUserScreen()
        {
            PushViewModel(new CreateOrEditUserViewModel(ViewModelChanger, SelectedUser, this) { CurrentUser = CurrentUser });
        }

        public void CreatedUser(User user)
        {
            Users.Add(user);
        }

        public void DeleteUser(User user)
        {
            user.Delete();
            Users.Remove(user);
        }
    }
}
