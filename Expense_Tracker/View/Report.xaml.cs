using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using Expense_Tracker.Report;
using Expense_Tracker.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Expense_Tracker.View
{
    /// <summary>
    /// Interaction logic for Report.xaml
    /// </summary>
    public partial class Report : UserControl
    {
        public Report()
        {
            InitializeComponent();

            // Subscribe vào event của ViewModel khi DataContext đã sẵn sàng
            Loaded += (s, e) =>
            {
                if (DataContext is ReportViewModel vm)
                {
                    vm.OnGenerateReport += GenerateReport;
                }
            };

            Unloaded += (s, e) =>
            {
                if (DataContext is ReportViewModel vm)
                {
                    vm.OnGenerateReport -= GenerateReport;
                }
            };
        }

        /// <summary>
        /// Nhận tham số từ ViewModel và tạo Crystal Report (code-behind only).
        /// </summary>
        /// <param name="reportDate">Ngày báo cáo (null nếu checkbox "Tất cả ngày" được chọn)</param>
        /// <param name="thang">Tháng được chọn từ ComboBox</param>
        /// <param name="nam">Năm được chọn từ ComboBox</param>
        private void GenerateReport(DateTime? reportDate, int thang, int nam)
        {
            try
            {
                MonthlyReports rpt = new MonthlyReports();

                // ── Thiết lập kết nối ──
                ConnectionInfo connInfo = new ConnectionInfo();
                connInfo.ServerName = @"Vu-Johan\SQLEXPRESS";
                connInfo.DatabaseName = "EXPENSE_TRACKER_DB";
                connInfo.IntegratedSecurity = true;

                foreach (Table table in rpt.Database.Tables)
                {
                    TableLogOnInfo logOnInfo = table.LogOnInfo;
                    logOnInfo.ConnectionInfo = connInfo;
                    table.ApplyLogOnInfo(logOnInfo);
                }

                // ── Truyền tham số từ ViewModel ──
                rpt.SetParameterValue("p_DateReport", reportDate ?? DateTime.Now);
                rpt.SetParameterValue("@Thang", thang);
                rpt.SetParameterValue("@Nam", nam);

                // ── Hiển thị báo cáo ──
                MainReport.ViewerCore.ReportSource = rpt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi xảy ra: " + ex.Message,
                    "Thông báo lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
