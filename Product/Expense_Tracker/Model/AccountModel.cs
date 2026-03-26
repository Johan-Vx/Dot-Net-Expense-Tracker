using Expense_Tracker.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Tracker.Model
{
    public class AccountModel:BaseViewModel
    {
        public int Id { get; set; }
        public string AccountName { get; set; }
        public string AccountType { get; set; }
        public decimal Balance { get; set; }
        public string BalanceText => $"đ {Balance:N2}";
        public string ColorHex { get; set; }
        public string IconText { get; set; }
    }
}
