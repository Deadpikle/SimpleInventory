using SimpleInventory.Helpers;
using SimpleInventory.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace SimpleInventory.ViewModels
{
    class MainWindowViewModel : ChangeNotifier, IChangeViewModel
    {
        BaseViewModel _currentViewModel;
        Stack<BaseViewModel> _viewModels;

        // https://stackoverflow.com/a/4970019 -- logic for inactivity
        private readonly DispatcherTimer _activityTimer;

        public MainWindowViewModel()
        {
            // upgrading settings: https://stackoverflow.com/a/534335
            if (Properties.Settings.Default.UpgradeRequired)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeRequired = false;
                Properties.Settings.Default.Save();
            }

            _viewModels = new Stack<BaseViewModel>();
            var initialViewModel = new LoginViewModel(this);
            _viewModels.Push(initialViewModel);
            CurrentViewModel = initialViewModel;
            // setup inactivity timer
            InputManager.Current.PreProcessInput += InputPreProcessInput;
            _activityTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(10), IsEnabled = true };
            _activityTimer.Tick += ActivityTimerTick;
        }

        public BaseViewModel CurrentViewModel
        {
            get { return _currentViewModel; }
            set { _currentViewModel = value; NotifyPropertyChanged(); }
        }

        private void ActivityTimerTick(object sender, EventArgs e)
        {
            // set UI to inactivity
            PopToBaseViewModel();
        }

        private void InputPreProcessInput(object sender, PreProcessInputEventArgs e)
        {
            InputEventArgs inputEventArgs = e.StagingItem.Input;

            if (inputEventArgs is MouseEventArgs || inputEventArgs is KeyboardEventArgs)
            {
                // reset timer
                _activityTimer.Stop();
                _activityTimer.Start();
            }
        }

        #region IChangeViewModel

        public void PushViewModel(BaseViewModel model)
        {
            _viewModels.Push(model);
            CurrentViewModel = model;
        }

        public void PopViewModel()
        {
            if (_viewModels.Count > 1)
            {
                _viewModels.Pop();
            }
            CurrentViewModel = _viewModels.Peek();
        }

        public void PopToBaseViewModel()
        {
            while (_viewModels.Count > 1)
            {
                _viewModels.Pop();
                CurrentViewModel = _viewModels.Peek();
            }
        }

        #endregion
    }
}
