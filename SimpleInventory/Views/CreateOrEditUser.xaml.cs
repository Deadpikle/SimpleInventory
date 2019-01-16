using SimpleInventory.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SimpleInventory.Views
{
    /// <summary>
    /// Interaction logic for CreateOrEditUser.xaml
    /// </summary>
    public partial class CreateOrEditUser : UserControl
    {
        public CreateOrEditUser()
        {
            InitializeComponent();
        }

        private void PasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var dataContext = DataContext as CreateOrEditUserViewModel;
            if (dataContext != null)
            {
                dataContext.Password = PasswordInput.SecurePassword;
            }
        }

        private void ConfirmPasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var dataContext = DataContext as CreateOrEditUserViewModel;
            if (dataContext != null)
            {
                dataContext.ConfirmPassword = ConfirmPasswordInput.SecurePassword;
            }
        }

        private void PasswordInput_KeyDown(object sender, KeyEventArgs e)
        {

        }
    }
}
