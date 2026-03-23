using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Phan1_4.View;
namespace Phan1_4.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private object _currentChildView;

        public object CurrentChildView
        {
            get => _currentChildView;
            set { _currentChildView = value; OnPropertyChanged(); }
        }
        public RelayCommand ShowDashBoardCommand { get; set; }
        public RelayCommand ShowBudgetsCommand { get;set; }
        public MainViewModel()
        { 
            CurrentChildView = new DashBoardView();
            ShowDashBoardCommand = new RelayCommand(o => CurrentChildView = new DashBoardView());
            ShowBudgetsCommand = new RelayCommand(o => CurrentChildView = new BudgetsView());
        }
    } 
}
