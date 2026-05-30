using Expense_Tracker.ViewModel;
using System.Windows.Controls;

namespace Expense_Tracker.View
{
    /// <summary>
    /// Interaction logic for PhieuThuChiView.xaml
    /// </summary>
    public partial class PhieuThuChiView : UserControl
    {
        public PhieuThuChiView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// When a cell edit ends (e.g. SoTien changes), trigger TongTien recalculation via ViewModel.
        /// </summary>
        private void ChiTietGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                // Delay dispatch to allow the binding to update first
                Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    if (DataContext is PhieuThuChiViewModel vm)
                    {
                        vm.TriggerRecalcTongTien();
                    }
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }
    }
}
