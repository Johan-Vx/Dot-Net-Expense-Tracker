using Expense_Tracker.Module;
using Expense_Tracker.Model;
using System;
using System.Windows.Input;

namespace Expense_Tracker.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        private BaseViewModel _currentView;
        public BaseViewModel CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public string CurrentUserName => SessionManager.CurrentUser?.HoTen;
        public string CurrentUserRole => SessionManager.IsAdmin ? "Admin" : "User";
        public bool IsAdmin => SessionManager.IsAdmin;

        public ICommand NavigateDashboardCommand { get; }
        public ICommand NavigatePhieuThuChiCommand { get; }
        public ICommand NavigateLedgerCommand { get; }
        public ICommand NavigateSettingsCommand { get; }
        public ICommand LogoutCommand { get; }

        public Action CloseAction { get; set; }

        public MainViewModel()
        {
            NavigationService.NavigateToViewModel = (vm) => CurrentView = vm;

            NavigateDashboardCommand = new RelayCommand(o => NavigationService.NavigateTo(new DashBoardViewModel()));
            NavigatePhieuThuChiCommand = new RelayCommand(o => NavigationService.NavigateTo(new PhieuThuChiViewModel()));
            NavigateLedgerCommand = new RelayCommand(o => NavigationService.NavigateTo(new LedgerViewModel()));
            NavigateSettingsCommand = new RelayCommand(o => NavigationService.NavigateTo(new SettingsViewModel()));
            
            LogoutCommand = new RelayCommand(o => 
            {
                SessionManager.CurrentUser = null;
                
                var loginView = new Expense_Tracker.View.LoginView();
                loginView.Show();
                CloseAction?.Invoke();
            });

            // Start with Dashboard
            NavigationService.NavigateTo(new DashBoardViewModel());
        }
    }
}
