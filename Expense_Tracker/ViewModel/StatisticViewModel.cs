using Expense_Tracker.Module;
using Expense_Tracker.Model;
using LiveCharts;
using LiveCharts.Wpf;
using System.Linq;
using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;

namespace Expense_Tracker.ViewModel
{
    public class StatisticViewModel : BaseViewModel
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

        // 1. Doughnut Chart (Tỷ lệ Phiếu Thu vs Phiếu Chi)
        public SeriesCollection DoughnutSeries { get; set; }
        public Func<ChartPoint, string> DoughnutLabelFormatter { get; set; }

        // 2. Pie Chart (Cơ cấu chi tiêu theo Danh mục)
        public SeriesCollection PieSeries { get; set; }
        public Func<ChartPoint, string> PieLabelFormatter { get; set; }

        // 3. Bar Chart (Top 5 Danh Mục Chi) - RowSeries
        public SeriesCollection BarSeries { get; set; }
        public string[] BarLabels { get; set; }
        public Func<double, string> BarFormatter { get; set; }

        // 4. Line Graph (Tổng Chi 6 tháng)
        public SeriesCollection SingleLineSeries { get; set; }
        public string[] SingleLineLabels { get; set; }
        public Func<double, string> SingleLineFormatter { get; set; }

        // 5. Column Series (Thu vs Chi 6 tháng)
        public SeriesCollection ColumnSeries { get; set; }
        public string[] ColumnLabels { get; set; }
        public Func<double, string> ColumnFormatter { get; set; }

        // 6. Line Series (Số lượng Phiếu Thu vs Chi 6 tháng)
        public SeriesCollection MultiLineSeries { get; set; }
        public string[] MultiLineLabels { get; set; }
        public Func<double, string> MultiLineFormatter { get; set; }
        
        // ── Recent Transactions ──────────────────────────────────────
        public ObservableCollection<PhieuThuChi> RecentTransactions { get; set; }

        public StatisticViewModel()
        {
            LoadData();
        }

        private void LoadData()
        {
            using (var context = new EXPENSE_TRACKER_DBEntities())
            {
                try
                {
                    // ── TỔNG QUAN ──
                    TongSoDu = context.Database
                        .SqlQuery<decimal>("SELECT dbo.func_LayTongSoDu()")
                        .FirstOrDefault();

                    var now = DateTime.Now;
                    
                    var thangNay = context.sproc_ThongKeThuChiThang(now.Month, now.Year).ToList();
                    TongThuThangNay = thangNay.FirstOrDefault(x => x.LoaiPhieu == "Thu")?.TongTienThang ?? 0m;
                    TongChiThangNay = thangNay.FirstOrDefault(x => x.LoaiPhieu == "Chi")?.TongTienThang ?? 0m;

                    // Fetch raw data (Eager Loading)
                    var sixMonthsAgo = new DateTime(now.Year, now.Month, 1).AddMonths(-5);
                    var allTransactions6Months = context.PhieuThuChi
                        .Include("ChiTietPhieu.DanhMuc")
                        .Where(p => p.NgayLap >= sixMonthsAgo)
                        .ToList();

                    // 1. Doughnut Chart: Số lượng Thu vs Chi (All Time)
                    var countThu = context.PhieuThuChi.Count(p => p.LoaiPhieu == "Thu");
                    var countChi = context.PhieuThuChi.Count(p => p.LoaiPhieu == "Chi");
                    DoughnutSeries = new SeriesCollection
                    {
                        new PieSeries { Title = "Phiếu Thu", Values = new ChartValues<int> { countThu }, Fill = System.Windows.Media.Brushes.MediumSeaGreen },
                        new PieSeries { Title = "Phiếu Chi", Values = new ChartValues<int> { countChi }, Fill = System.Windows.Media.Brushes.IndianRed }
                    };
                    DoughnutLabelFormatter = cp => $"{cp.SeriesView.Title}: {cp.Y} phiếu";

                    // 2. Pie Chart: Cơ cấu chi tiêu theo danh mục (6 months)
                    var chiTiet6Months = allTransactions6Months
                        .Where(p => p.LoaiPhieu == "Chi")
                        .SelectMany(p => p.ChiTietPhieu)
                        .GroupBy(c => c.DanhMuc.TenDM)
                        .Select(g => new { TenDM = g.Key, TongTien = g.Sum(c => c.SoTien) })
                        .OrderByDescending(x => x.TongTien)
                        .Take(5)
                        .ToList();

                    PieSeries = new SeriesCollection();
                    foreach (var item in chiTiet6Months)
                    {
                        PieSeries.Add(new PieSeries { Title = item.TenDM, Values = new ChartValues<decimal> { item.TongTien } });
                    }
                    PieLabelFormatter = cp => $"{cp.SeriesView.Title}: {cp.Y:N0} đ";

                    // 3. Bar Chart: Top 5 danh mục chi
                    BarSeries = new SeriesCollection
                    {
                        new RowSeries
                        {
                            Title = "Số tiền chi",
                            Values = new ChartValues<decimal>(chiTiet6Months.Select(x => x.TongTien).Reverse()),
                            Fill = System.Windows.Media.Brushes.SteelBlue
                        }
                    };
                    BarLabels = chiTiet6Months.Select(x => x.TenDM).Reverse().ToArray();
                    BarFormatter = value => value.ToString("N0") + " đ";

                    // Prepare monthly groupings (6 months)
                    var months = new List<string>();
                    var thuAmounts = new ChartValues<decimal>();
                    var chiAmounts = new ChartValues<decimal>();
                    var thuCounts = new ChartValues<int>();
                    var chiCounts = new ChartValues<int>();

                    for (int i = 5; i >= 0; i--)
                    {
                        var m = now.AddMonths(-i);
                        months.Add(m.ToString("MM/yyyy"));
                        
                        var txInMonth = allTransactions6Months.Where(p => p.NgayLap.Month == m.Month && p.NgayLap.Year == m.Year).ToList();
                        
                        thuAmounts.Add(txInMonth.Where(p => p.LoaiPhieu == "Thu").Sum(p => p.TongTien));
                        chiAmounts.Add(txInMonth.Where(p => p.LoaiPhieu == "Chi").Sum(p => p.TongTien));
                        
                        thuCounts.Add(txInMonth.Count(p => p.LoaiPhieu == "Thu"));
                        chiCounts.Add(txInMonth.Count(p => p.LoaiPhieu == "Chi"));
                    }

                    // 4. Line Graph: Tổng Chi 6 tháng
                    SingleLineSeries = new SeriesCollection
                    {
                        new LineSeries
                        {
                            Title = "Tổng Chi",
                            Values = chiAmounts,
                            PointGeometrySize = 12,
                            Stroke = System.Windows.Media.Brushes.IndianRed,
                            Fill = System.Windows.Media.Brushes.Transparent
                        }
                    };
                    SingleLineLabels = months.ToArray();
                    SingleLineFormatter = value => value.ToString("N0") + " đ";

                    // 5. Column Series: So sánh Thu vs Chi
                    ColumnSeries = new SeriesCollection
                    {
                        new ColumnSeries { Title = "Thu", Values = thuAmounts, Fill = System.Windows.Media.Brushes.MediumSeaGreen },
                        new ColumnSeries { Title = "Chi", Values = chiAmounts, Fill = System.Windows.Media.Brushes.IndianRed }
                    };
                    ColumnLabels = months.ToArray();
                    ColumnFormatter = value => value.ToString("N0") + " đ";

                    // 6. Line Series (2 đường): Số lượng Phiếu Thu vs Chi
                    MultiLineSeries = new SeriesCollection
                    {
                        new LineSeries { Title = "Số phiếu Thu", Values = thuCounts, Stroke = System.Windows.Media.Brushes.MediumSeaGreen, Fill = System.Windows.Media.Brushes.Transparent },
                        new LineSeries { Title = "Số phiếu Chi", Values = chiCounts, Stroke = System.Windows.Media.Brushes.IndianRed, Fill = System.Windows.Media.Brushes.Transparent }
                    };
                    MultiLineLabels = months.ToArray();
                    MultiLineFormatter = value => value.ToString("N0") + " phiếu";

                    // ── Giao dịch gần đây ──
                    var recent = context.PhieuThuChi
                        .Include("TaiKhoanQuy")
                        .Include("NguoiDung")
                        .OrderByDescending(p => p.NgayLap)
                        .Take(10)
                        .ToList();

                    RecentTransactions = new ObservableCollection<PhieuThuChi>(recent);
                }
                catch (Exception ex)
                {
                    // Fallback on error
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    DoughnutSeries = new SeriesCollection();
                    PieSeries = new SeriesCollection();
                    BarSeries = new SeriesCollection();
                    SingleLineSeries = new SeriesCollection();
                    ColumnSeries = new SeriesCollection();
                    MultiLineSeries = new SeriesCollection();
                    RecentTransactions = new ObservableCollection<PhieuThuChi>();
                }
            }
        }
    }
}
