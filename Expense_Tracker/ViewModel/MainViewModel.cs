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
        private bool? _dialogResult;
        public bool? DialogResult
        {
            get => _dialogResult;
            set
            {
                _dialogResult = value;
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

        public MainViewModel()
        {
            NavigationService.NavigateToViewModel = (vm) => CurrentView = vm;

            NavigateDashboardCommand = new RelayCommand(o => NavigationService.NavigateTo(new DashBoardViewModel()));
            NavigatePhieuThuChiCommand = new RelayCommand(o => NavigationService.NavigateTo(new PhieuThuChiViewModel()));
            NavigateLedgerCommand = new RelayCommand(o => NavigationService.NavigateTo(new LedgerViewModel()));
            NavigateSettingsCommand = new RelayCommand(o => NavigationService.NavigateTo(new SettingsViewModel()));

            LogoutCommand = new RelayCommand(ExecuteLogout);
            NavigationService.NavigateTo(new DashBoardViewModel());
        }
        private void ExecuteLogout(object obj)
        {
            // Xóa phiên đăng nhập hiện tại
            SessionManager.CurrentUser = null;
            DialogResult = true;
        }
    }
}