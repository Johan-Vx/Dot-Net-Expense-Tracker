using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace Expense_Tracker.View
{
    /// <summary>
    /// Interaction logic for BudgetView.xaml
    /// </summary>
    public partial class BudgetView : UserControl
    {
        public static readonly IValueConverter OverBudgetConverter = new OverBudgetConverterImpl();

        public BudgetView()
        {
            InitializeComponent();
        }

        private class OverBudgetConverterImpl : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is double percentage)
                {
                    return percentage > 100;
                }
                return false;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }
}
