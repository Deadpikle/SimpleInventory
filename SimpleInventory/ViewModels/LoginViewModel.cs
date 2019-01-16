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
            catch { }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
            return "";
        }

        private void TryLogin()
        {
            if (string.IsNullOrWhiteSpace(Username))
            {
                Error = "Username is required";
            }
            else if (Password == null)
            {
                Error = "Password is required";
            }
            else
            {
                var user = User.LoadUser(Username, SecureStringToString(Password));
                if (user != null)
                {
                    Username = "";
                    Password.Clear();
                    Error = "";
                    PushViewModel(new HomeScreenViewModel(ViewModelChanger) { User = user });
                }
                else
                {
                    Error = "Invalid username or password";
                }
            }
        }
    }
}
