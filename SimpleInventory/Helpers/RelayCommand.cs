using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SimpleInventory.Helpers
{
    // https://www.codeproject.com/Articles/126249/MVVM-Pattern-in-WPF-A-Simple-Tutorial-for-Absolute
    class RelayCommand : ICommand
    {
        private Action<object> _action;

        public RelayCommand(Action<object> action)
        {
            _action = action;
        }

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
           ///if (parameter != null)
            {
                _action(parameter);
            }
        }

        #endregion
    }
}
