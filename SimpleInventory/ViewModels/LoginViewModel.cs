using SimpleInventory.Helpers;
using SimpleInventory.Interfaces;
using SimpleInventory.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SimpleInventory.ViewModels
{
    class LoginViewModel : BaseViewModel
    {
        private string _username;
        private SecureString _password;
        private string _error;

        public LoginViewModel(IChangeViewModel viewModelChanger) : base(viewModelChanger)
        {
        }

        public string Username
        {
            get { return _username; }
            set { _username = value; NotifyPropertyChanged(); }
        }

        public SecureString Password
        {
            private get { return _password; }
            set { _password = value; NotifyPropertyChanged(); }
        }

        public string Error
        {
            get { return _error; }
            set { _error = value; NotifyPropertyChanged(); }
        }

        public ICommand AttemptLogin
        {
            get { return new RelayCommand(TryLogin); }
        }

        // https://stackoverflow.com/a/819705
        // I don't care _that_ much about this string being in RAM for a short time. :)
        private string SecureStringToString(SecureString value)
        {
            IntPtr valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(Password);
                return Marshal.PtrToStringUni(valuePtr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }

        private void TryLogin()
        {
            var user = User.LoadUser(Username, SecureStringToString(Password));
            if (user != null)
            {
                PushViewModel(new HomeScreenViewModel(ViewModelChanger));
            }
            else
            {
                Error = "Invalid username or password";
            }
        }
    }
}
