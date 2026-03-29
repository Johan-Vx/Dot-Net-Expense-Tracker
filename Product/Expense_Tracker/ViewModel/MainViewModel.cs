using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Expense_Tracker.View;
using Expense_Tracker.Module;
namespace Expense_Tracker.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        private object _currentChildView;
        public object CurrentChildView
        {
            get => _currentChildView;
            set { _currentChildView = value; OnPropertyChanged(); }
        }
        public RelayCommand ShowDashBoardCommand { get; }
        public RelayCommand ShowBudgetsCommand { get; }
        public RelayCommand ShowAccountsCommand { get; }
        public RelayCommand ShowTransactionCommand { get; }
        public MainViewModel()
        { 
            CurrentChildView = new DashBoardView();
            ShowDashBoardCommand = new RelayCommand(_ =>  CurrentChildView = new DashBoardView());
            ShowBudgetsCommand=new RelayCommand(_ => CurrentChildView=new BudgetsView());
            ShowAccountsCommand = new RelayCommand(_ => CurrentChildView = new AccoutntsViewModel());
            ShowTransactionCommand = new RelayCommand(_ => CurrentChildView = new TransactionsViewModel());
        }
    } 
}
