using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Tracker.Model
{
    public class Budget
    {
        public string CategoryName { get; set; }
        public double LimitAmount { get; set; }
        public double SpentAmount { get; set; }
        public string Month { get; set; }

        public double ProgressValue => (SpentAmount / LimitAmount) * 100;

        public string ProgressColor
        {
            get
            {
                if (ProgressValue >= 100) return "#FF4757";
                if (ProgressValue >= 80) return "#FFA502"; 
                return "#2ED573";                      
            }
        }
    }
}
