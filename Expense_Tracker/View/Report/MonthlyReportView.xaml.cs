using System;
using System.Windows;
using System.Windows.Controls;
using Expense_Tracker.ViewModel;
using Expense_Tracker.Report;

namespace Expense_Tracker.View
{
    /// <summary>
    /// Interaction logic for MonthlyReportView.xaml
    /// </summary>
    public partial class MonthlyReportView : UserControl
    {
        public MonthlyReportView()
        {
            InitializeComponent();
            this.DataContextChanged += MonthlyReportView_DataContextChanged;
        }

        private void MonthlyReportView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ReportViewModel oldVm)
            {
                oldVm.OnGenerateReport -= ReportViewModel_OnGenerateReport;
            }
            if (e.NewValue is ReportViewModel newVm)
            {
                newVm.OnGenerateReport += ReportViewModel_OnGenerateReport;
                
                // Tự động tạo báo cáo lần đầu tiên khi View được load và DataContext được gán.
                // Sử dụng Dispatcher.BeginInvoke để chạy bất đồng bộ nhằm tránh lỗi Dispatcher processing suspended
                // khi phát sinh lỗi/hiển thị MessageBox trong luồng DataContextChanged của WPF layout.
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (newVm.GenerateReportCommand != null && newVm.GenerateReportCommand.CanExecute(null))
                    {
                        newVm.GenerateReportCommand.Execute(null);
                    }
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private void ReportViewModel_OnGenerateReport(DateTime? dateParam, DateTime tuNgay, DateTime denNgay)
        {
            try
            {
                MonthlyReports rpt = new MonthlyReports();
                
                // Thiết lập tham số cho report
                if (dateParam.HasValue)
                {
                    rpt.SetParameterValue("p_DateReport", dateParam.Value);
                }
                else
                {
                    rpt.SetParameterValue("p_DateReport", DateTime.Today);
                }
                
                rpt.SetParameterValue("@TuNgay", tuNgay);
                rpt.SetParameterValue("@DenNgay", denNgay);
                
                // Gán ReportSource cho CrystalReportsViewer
                MainReport.ViewerCore.ReportSource = rpt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hiển thị báo cáo: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
