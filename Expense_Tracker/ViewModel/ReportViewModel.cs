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

        // ── DatePicker Từ Ngày ───────────────────────────────────────
        private DateTime _selectedStartTime;
        public DateTime SelectedStartTime
        {
            get => _selectedStartTime;
            set { _selectedStartTime = value; OnPropertyChanged(); }
        }

        // ── DatePicker Đến Ngày ───────────────────────────────────────
        private DateTime _selectedEndTime;
        public DateTime SelectedEndTime
        {
            get => _selectedEndTime;
            set { _selectedEndTime = value; OnPropertyChanged(); }
        }

        // ── Command ─────────────────────────────────────────────────
        public ICommand GenerateReportCommand { get; }

        // ── Event để code-behind nhận tham số tạo report ────────────
        public event Action<DateTime?, DateTime, DateTime> OnGenerateReport;

        public ReportViewModel()
        {
            _context = new EXPENSE_TRACKER_DBEntities();
            InitializeDates();

            GenerateReportCommand = new RelayCommand(_ =>
            {
                DateTime? dateParam = IsAllDate ? (DateTime?)null : SelectedDate;
                OnGenerateReport?.Invoke(dateParam, SelectedStartTime, SelectedEndTime);
            });
        }

        /// <summary>
        /// Khởi tạo khoảng thời gian lọc báo cáo mặc định
        /// </summary>
        private void InitializeDates()
        {
            SelectedStartTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            SelectedEndTime = DateTime.Today;

            try
            {
                if (_context.PhieuThuChi.Any())
                {
                    var minDate = _context.PhieuThuChi.Min(p => p.NgayLap);
                    var maxDate = _context.PhieuThuChi.Max(p => p.NgayLap);

                    SelectedStartTime = minDate;
                    SelectedEndTime = maxDate;
                }
            }
            catch
            {
                // Fallback nếu lỗi DB
            }
        }
    }
}
