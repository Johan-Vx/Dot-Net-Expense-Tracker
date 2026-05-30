using Expense_Tracker.Module;
using Expense_Tracker.Model;
using LiveCharts;
using LiveCharts.Wpf;
using System.Linq;
using System.Collections.ObjectModel;
using System;
using System.Windows.Media;

namespace Expense_Tracker.ViewModel
{
    public class DashBoardViewModel : BaseViewModel
    {
        // ── Summary Cards ────────────────────────────────────────────
        private decimal _tongSoDu;
        public decimal TongSoDu
        {
            get => _tongSoDu;
            set { _tongSoDu = value; OnPropertyChanged(); }
        }

        private decimal _tongThuThangNay;
        public decimal TongThuThangNay
        {
            get => _tongThuThangNay;
            set { _tongThuThangNay = value; OnPropertyChanged(); }
        }

        private decimal _tongChiThangNay;
        public decimal TongChiThangNay
        {
            get => _tongChiThangNay;
            set { _tongChiThangNay = value; OnPropertyChanged(); }
        }

        // ── Doughnut Chart ───────────────────────────────────────────
        private ChartValues<double> _chiThangNayValues;
        public ChartValues<double> ChiThangNayValues
        {
            get => _chiThangNayValues;
            set { _chiThangNayValues = value; OnPropertyChanged(); }
        }

        private ChartValues<double> _chiThangTruocValues;
        public ChartValues<double> ChiThangTruocValues
        {
            get => _chiThangTruocValues;
            set { _chiThangTruocValues = value; OnPropertyChanged(); }
        }

        private string _phanTramThayDoi;
        public string PhanTramThayDoi
        {
            get => _phanTramThayDoi;
            set { _phanTramThayDoi = value; OnPropertyChanged(); }
        }

        private bool _isChiTang;
        public bool IsChiTang
        {
            get => _isChiTang;
            set { _isChiTang = value; OnPropertyChanged(); }
        }

        public string[] DonutLabels { get; set; }
        public Func<ChartPoint, string> DonutLabelFormatter { get; set; }

        // ── Recent Transactions ──────────────────────────────────────
        public ObservableCollection<PhieuThuChi> RecentTransactions { get; set; }

        public DashBoardViewModel()
        {
            LoadData();
        }

        private void LoadData()
        {
            using (var context = new EXPENSE_TRACKER_DBEntities())
            {
                try
                {
                    // ── Tổng số dư ──
                    TongSoDu = context.Database
                        .SqlQuery<decimal>("SELECT dbo.func_LayTongSoDu()")
                        .FirstOrDefault();

                    var now = DateTime.Now;

                    // ── Tháng này ──
                    var thangNay = context.sp_ThongKeThuChiThang(now.Month, now.Year).ToList();
                    TongThuThangNay = thangNay.FirstOrDefault(x => x.LoaiPhieu == "Thu")?.TongTienThang ?? 0m;
                    TongChiThangNay = thangNay.FirstOrDefault(x => x.LoaiPhieu == "Chi")?.TongTienThang ?? 0m;

                    // ── Tháng trước (LINQ trực tiếp, an toàn với null) ──
                    var thangTruoc = now.AddMonths(-1);
                    var tongChiThangTruoc = context.PhieuThuChi
                        .Where(p => p.LoaiPhieu == "Chi"
                                 && p.NgayLap.Month == thangTruoc.Month
                                 && p.NgayLap.Year  == thangTruoc.Year)
                        .Select(p => (decimal?)p.TongTien)
                        .Sum() ?? 0m;

                    // ── Tính % thay đổi ──
                    CalculatePhanTram(TongChiThangNay, tongChiThangTruoc);

                    // ── Doughnut series values ──
                    ChiThangNayValues  = new ChartValues<double> { (double)TongChiThangNay };
                    ChiThangTruocValues = new ChartValues<double> { (double)tongChiThangTruoc };

                    DonutLabels = new[] { "Tháng này", "Tháng trước" };
                    DonutLabelFormatter = cp => cp.SeriesView.Title + ": " + cp.Y.ToString("N0") + " đ";

                    // ── Recent Transactions ──
                    var recent = context.PhieuThuChi
                        .Include("TaiKhoanQuy")
                        .Include("NguoiDung")
                        .OrderByDescending(p => p.NgayLap)
                        .Take(10)
                        .ToList();

                    RecentTransactions = new ObservableCollection<PhieuThuChi>(recent);
                }
                catch
                {
                    // Graceful fallback
                    TongChiThangNay     = 0;
                    ChiThangNayValues   = new ChartValues<double> { 0 };
                    ChiThangTruocValues = new ChartValues<double> { 0 };
                    PhanTramThayDoi     = "N/A";
                    RecentTransactions  = new ObservableCollection<PhieuThuChi>();
                }
            }
        }

        /// <summary>
        /// Tính % thay đổi chi tiêu so với tháng trước. 
        /// Nếu tháng trước = 0 thì hiển thị "Mới" thay vì chia 0.
        /// </summary>
        private void CalculatePhanTram(decimal thangNay, decimal thangTruoc)
        {
            if (thangTruoc == 0)
            {
                PhanTramThayDoi = thangNay > 0 ? "Mới" : "0%";
                IsChiTang = true;
                return;
            }

            var delta  = thangNay - thangTruoc;
            var phanTram = Math.Round((delta / thangTruoc) * 100, 1);
            IsChiTang      = phanTram >= 0;
            PhanTramThayDoi = (phanTram >= 0 ? "+" : "") + phanTram + "%";
        }
    }
}
