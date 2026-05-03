using Expense_Tracker.View;
using Expense_Tracker.ViewModel;
using Expense_Tracker.Module;
using System.Windows;

namespace Expense_Tracker
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            this.Loaded += (s, e) =>
            {
                if (SessionManager.CurrentUser == null)
                {
                    this.Hide();
                    LoginView login = new LoginView();
                    bool? res = login.ShowDialog();
                    if (res == true)
                    {
                        this.Show();
                        var vm = new MainViewModel();
                        vm.CloseAction = () => this.Close();
                        this.DataContext = vm;
                    }
                    else
                    {
                        Application.Current.Shutdown();
                    }
                }
            };
        }
    }
}
