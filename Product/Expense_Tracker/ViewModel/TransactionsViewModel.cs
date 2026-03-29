using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Expense_Tracker.Model;

namespace Expense_Tracker.ViewModel
{
    public class TransactionsViewModel
    {
        public ObservableCollection<TransactionModel> Transactions { get; set; }

        public TransactionsViewModel()
        {
            Transactions = new ObservableCollection<TransactionModel>
            {
                new TransactionModel { GhiChu="Ăn sáng", ThoiGian="10/03/2025", SoTienText="-50,000 đ"},
                new TransactionModel { GhiChu="Ăn trưa", ThoiGian="10/03/2025", SoTienText="-30,000 đ"},
                new TransactionModel { GhiChu="Lương", ThoiGian="10/03/2025", SoTienText="+7,000,000 đ"},
            };
        }
    }
}
