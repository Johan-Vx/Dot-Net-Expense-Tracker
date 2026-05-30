using Expense_Tracker.Model;
using Expense_Tracker.Module;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Expense_Tracker.ViewModel
{
    public class ReportViewModel : BaseViewModel
    {
        private EXPENSE_TRACKER_DB_Entities _context;

        public ObservableCollection<TaiKhoanQuy> DanhSachQuy { get; set; }

        private string _selectedMaQuy;
        public string SelectedMaQuy
        {
            get => _selectedMaQuy;
            set { _selectedMaQuy = value; OnPropertyChanged(); }
        }

        private DateTime? _selectedDate;
        public DateTime? SelectedDate
        {
            get => _selectedDate;
            set { _selectedDate = value; OnPropertyChanged(); IsAllDate = !value.HasValue; }
        }

        private bool _isAllDate = true;
        public bool IsAllDate
        {
            get => _isAllDate;
            set { _isAllDate = value; OnPropertyChanged(); }
        }

        private DateTime _selectedStartTime;
        public DateTime SelectedStartTime
        {
            get => _selectedStartTime;
            set { _selectedStartTime = value; OnPropertyChanged(); }
        }

        private DateTime _selectedEndTime;
        public DateTime SelectedEndTime
        {
            get => _selectedEndTime;
            set { _selectedEndTime = value; OnPropertyChanged(); }
        }

        public ICommand GenerateThuChiCommand { get; }
        public ICommand GenerateSaoKeCommand { get; }

        public event Action<string, DateTime, DateTime, DateTime, string> OnGenerateReport;

        public ReportViewModel()
        {
            _context = new EXPENSE_TRACKER_DB_Entities();
            LoadDanhSachQuy();
            InitializeDates();

            GenerateThuChiCommand = new RelayCommand(_ => ExecuteGenerateReport("ThuChi"));
            GenerateSaoKeCommand = new RelayCommand(_ => ExecuteGenerateReport("SaoKe"));
        }

        private void LoadDanhSachQuy()
        {
            try
            {
                DanhSachQuy = new ObservableCollection<TaiKhoanQuy>(_context.TaiKhoanQuy.ToList());
            }
            catch { DanhSachQuy = new ObservableCollection<TaiKhoanQuy>(); }
        }

        private void ExecuteGenerateReport(string type)
        {
            if (SelectedStartTime > SelectedEndTime)
            {
                MessageBox.Show("Ngày bắt đầu không được lớn hơn ngày kết thúc!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (type == "SaoKe" && string.IsNullOrEmpty(SelectedMaQuy))
            {
                MessageBox.Show("Vui lòng chọn quỹ để thực hiện sao kê!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime dateParam = SelectedDate ?? DateTime.Today;
            OnGenerateReport?.Invoke(type, dateParam, SelectedStartTime, SelectedEndTime, SelectedMaQuy);
        }

        private void InitializeDates()
        {
            SelectedStartTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            SelectedEndTime = DateTime.Today;

            try
            {
                if (_context.PhieuThuChi.Any())
                {
                    SelectedStartTime = _context.PhieuThuChi.Min(p => p.NgayLap);
                    SelectedEndTime = _context.PhieuThuChi.Max(p => p.NgayLap);
                }
            }
            catch { }
        }
    }
}
