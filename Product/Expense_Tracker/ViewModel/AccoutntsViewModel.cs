using Expense_Tracker.Model;
using Expense_Tracker.Module;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Tracker.ViewModel
{
    public class AccoutntsViewModel:BaseViewModel
    {
        public ObservableCollection<AccountModel> Accounts { get; set; }

        public AccoutntsViewModel()
        {
            Accounts = new ObservableCollection<AccountModel>
            {
                new AccountModel
                {
                AccountName = "Wallet",
                AccountType = "Cash",
                Balance = 2513000,
                ColorHex = "#4CAF50", // Màu xanh lá
                IconText = "💳"
                },
                new AccountModel
                {
                AccountName = "My bank",
                AccountType = "Savings account",
                Balance = 2802744,
                ColorHex = "#FFB300", // Màu vàng
                IconText = "🏦"
                }
            };      
        }
    }
}
