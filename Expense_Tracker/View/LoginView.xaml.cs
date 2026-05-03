using Expense_Tracker.ViewModel;
using System.Windows;

namespace Expense_Tracker.View
{
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
            var vm = new LoginViewModel();
            vm.CloseAction = (result) =>
            {
                this.DialogResult = result;
                this.Close();
            };
            this.DataContext = vm;
        }
    }
}
