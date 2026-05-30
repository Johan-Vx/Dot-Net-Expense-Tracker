using CrystalDecisions.CrystalReports.Engine;
using Expense_Tracker.Model;
using Expense_Tracker.Module;
using Expense_Tracker.Report;
using Expense_Tracker.ViewModel;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Expense_Tracker.View
{
    /// <summary>
    /// Interaction logic for ReportView.xaml
    /// </summary>
    public partial class ReportView : UserControl
    {
        public ReportView()
        {
            InitializeComponent();
            this.Loaded += ReportView_Loaded;
        }

        private void ReportView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ReportViewModel vm)
            {
                vm.OnGenerateReport -= OnGenerateReport;
                vm.OnGenerateReport += OnGenerateReport;
            }
        }

        private void OnGenerateReport(string type, DateTime dateParam, DateTime startTime, DateTime endTime, string maQuy)
        {
            try
            {
                using (var context = new EXPENSE_TRACKER_DB_Entities())
                {
                    ReportDocument report=new ReportDocument();
                    DotNetEnv.Env.Load();
                    string db_user = Environment.GetEnvironmentVariable("DB_USER");
                    string db_password = Environment.GetEnvironmentVariable("DB_PASSWORD");
                    string db_severname = Environment.GetEnvironmentVariable("DB_SERVERNAME");
                    string db_dbname = Environment.GetEnvironmentVariable("DB_DBNAME");

                    string reportName = type == "ThuChi" ? "MonthlyReports.rpt" : "FundStatementReportrpt.rpt";
                    string reportPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Report", reportName);
                    report.Load(reportPath);
                    report.SetDatabaseLogon(db_user, db_password, db_severname, db_dbname);                    

                    if (type == "ThuChi")
                    {
                        report.SetParameterValue("p_DateReport", DateTime.Now);
                        report.SetParameterValue("@TuNgay", startTime);
                        report.SetParameterValue("@DenNgay", endTime);
                        report.SetParameterValue("p_ReportPerson", SessionManager.CurrentUser?.HoTen ?? "");
                    }
                    else // "SaoKe"
                    {
                        report.SetParameterValue("p_DateReport", DateTime.Now);
                        report.SetParameterValue("@MaQuy", maQuy ?? "");
                        report.SetParameterValue("@TuNgay", startTime);
                        report.SetParameterValue("@DenNgay", endTime);
                        report.SetParameterValue("p_ReportPerson", SessionManager.CurrentUser?.HoTen ?? "");
                    }

                    MainReport.ViewerCore.ReportSource = report;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tạo báo cáo: " + ex.Message,
                    "Lỗi báo cáo", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
