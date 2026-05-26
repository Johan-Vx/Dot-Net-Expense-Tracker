using Expense_Tracker.Model;
using Expense_Tracker.Module;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Expense_Tracker.ViewModel
{
    public class ReportViewModel : BaseViewModel
    {
        private EXPENSE_TRACKER_DBEntities _context;

        // ── DatePicker ──────────────────────────────────────────────
        private DateTime? _selectedDate;
        public DateTime? SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged();
                // Tự động bật checkbox nếu không chọn ngày
                IsAllDate = !value.HasValue;
            }
        }

        // ── CheckBox "Tất cả ngày" ──────────────────────────────────
        private bool _isAllDate = true;
        public bool IsAllDate
        {
            get => _isAllDate;
            set
            {
                _isAllDate = value;
                OnPropertyChanged();
            }
        }

        // ── ComboBox Tháng ──────────────────────────────────────────
        private ObservableCollection<int> _months;
        public ObservableCollection<int> Months
        {
            get => _months;
            set { _months = value; OnPropertyChanged(); }
        }

        private int _selectedMonth;
        public int SelectedMonth
        {
            get => _selectedMonth;
            set { _selectedMonth = value; OnPropertyChanged(); }
        }

        // ── ComboBox Năm ────────────────────────────────────────────
        private ObservableCollection<int> _years;
        public ObservableCollection<int> Years
        {
            get => _years;
            set { _years = value; OnPropertyChanged(); }
        }

        private int _selectedYear;
        public int SelectedYear
        {
            get => _selectedYear;
            set { _selectedYear = value; OnPropertyChanged(); }
        }

        // ── Command ─────────────────────────────────────────────────
        public ICommand GenerateReportCommand { get; }

        // ── Event để code-behind nhận tham số tạo report ────────────
        public event Action<DateTime?, int, int> OnGenerateReport;

        public ReportViewModel()
        {
            _context = new EXPENSE_TRACKER_DBEntities();
            LoadMonthsAndYears();

            GenerateReportCommand = new RelayCommand(_ =>
            {
                DateTime? dateParam = IsAllDate ? (DateTime?)null : SelectedDate;
                OnGenerateReport?.Invoke(dateParam, SelectedMonth, SelectedYear);
            });
        }

        /// <summary>
        /// Lấy danh sách Tháng và Năm có dữ liệu từ bảng PhieuThuChi.
        /// </summary>
        private void LoadMonthsAndYears()
        {
            var phieuDates = _context.PhieuThuChi
                .Select(p => p.NgayLap)
                .ToList();

            // Lấy danh sách tháng distinct, sắp xếp tăng dần
            var months = phieuDates
                .Select(d => d.Month)
                .Distinct()
                .OrderBy(m => m)
                .ToList();

            // Lấy danh sách năm distinct, sắp xếp giảm dần (mới nhất lên trước)
            var years = phieuDates
                .Select(d => d.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToList();

            Months = new ObservableCollection<int>(months);
            Years = new ObservableCollection<int>(years);

            // Mặc định chọn tháng/năm hiện tại nếu có trong danh sách
            SelectedMonth = months.Contains(DateTime.Now.Month) ? DateTime.Now.Month : (months.Any() ? months.First() : 1);
            SelectedYear = years.Contains(DateTime.Now.Year) ? DateTime.Now.Year : (years.Any() ? years.First() : DateTime.Now.Year);
        }
    }
}
