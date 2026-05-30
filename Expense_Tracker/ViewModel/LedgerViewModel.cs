using Expense_Tracker.Module;
using Expense_Tracker.Model;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System;

namespace Expense_Tracker.ViewModel
{
    public class LedgerViewModel : BaseViewModel
    {
        private EXPENSE_TRACKER_DBEntities _context;

        // ── Date range filters ──
        private DateTime? _startDate;
        public DateTime? StartDate
        {
            get => _startDate;
            set { _startDate = value; OnPropertyChanged(); }
        }

        private DateTime? _endDate;
        public DateTime? EndDate
        {
            get => _endDate;
            set { _endDate = value; OnPropertyChanged(); }
        }

        // ── Filter 1: Transaction Type ──
        public ObservableCollection<string> LoaiPhieuOptions { get; set; }
            = new ObservableCollection<string> { "Tất cả", "Thu", "Chi" };

        private string _selectedLoaiPhieu = "Tất cả";
        public string SelectedLoaiPhieu
        {
            get => _selectedLoaiPhieu;
            set { _selectedLoaiPhieu = value; OnPropertyChanged(); }
        }

        // ── Filter 2: Fund Account ──
        public ObservableCollection<TaiKhoanQuy> TaiKhoanQuys { get; set; }

        private string _selectedMaQuy;
        public string SelectedMaQuy
        {
            get => _selectedMaQuy;
            set { _selectedMaQuy = value; OnPropertyChanged(); }
        }

        // ── Filter 3: Category ──
        public ObservableCollection<DanhMuc> DanhMucs { get; set; }

        private string _selectedMaDM;
        public string SelectedMaDM
        {
            get => _selectedMaDM;
            set { _selectedMaDM = value; OnPropertyChanged(); }
        }

        // ── Filter 4: General Search ──
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); }
        }

        // ── Filter 5: Amount Range ──
        private string _minAmountText;
        public string MinAmountText
        {
            get => _minAmountText;
            set { _minAmountText = value; OnPropertyChanged(); }
        }

        private string _maxAmountText;
        public string MaxAmountText
        {
            get => _maxAmountText;
            set { _maxAmountText = value; OnPropertyChanged(); }
        }

        // ── Result collection ──
        private ObservableCollection<PhieuThuChi> _phieuThuChis;
        public ObservableCollection<PhieuThuChi> PhieuThuChis
        {
            get => _phieuThuChis;
            set { _phieuThuChis = value; OnPropertyChanged(); }
        }

        // ── Commands ──
        public ICommand FilterCommand { get; }
        public ICommand ClearFilterCommand { get; }

        public LedgerViewModel()
        {
            _context = new EXPENSE_TRACKER_DBEntities();

            // Default filter: current month
            var now = DateTime.Now;
            StartDate = new DateTime(now.Year, now.Month, 1);
            EndDate = StartDate.Value.AddMonths(1).AddDays(-1);

            // Load lookup data
            TaiKhoanQuys = new ObservableCollection<TaiKhoanQuy>(_context.TaiKhoanQuy.ToList());
            DanhMucs = new ObservableCollection<DanhMuc>(_context.DanhMuc.ToList());

            FilterCommand = new RelayCommand(o => LoadData());
            ClearFilterCommand = new RelayCommand(o => ClearFilters());

            LoadData();
        }

        private void LoadData()
        {
            var query = _context.PhieuThuChi
                .Include("TaiKhoanQuy")
                .Include("NguoiDung")
                .Include("ChiTietPhieu")
                .AsQueryable();

            // Date range
            if (StartDate.HasValue)
                query = query.Where(p => p.NgayLap >= StartDate.Value);

            if (EndDate.HasValue)
            {
                var endOfDay = EndDate.Value.Date.AddDays(1).AddSeconds(-1);
                query = query.Where(p => p.NgayLap <= endOfDay);
            }

            // Filter 1: LoaiPhieu
            if (!string.IsNullOrEmpty(SelectedLoaiPhieu) && SelectedLoaiPhieu != "Tất cả")
                query = query.Where(p => p.LoaiPhieu == SelectedLoaiPhieu);

            // Filter 2: MaQuy
            if (!string.IsNullOrEmpty(SelectedMaQuy))
                query = query.Where(p => p.MaQuy == SelectedMaQuy);

            // Filter 3: MaDM (via ChiTietPhieu join)
            if (!string.IsNullOrEmpty(SelectedMaDM))
                query = query.Where(p => p.ChiTietPhieu.Any(ct => ct.MaDM == SelectedMaDM));

            // Filter 4: SearchText across SoPhieu, NguoiNopNhan, LyDoChung
            if (!string.IsNullOrEmpty(SearchText))
            {
                var term = SearchText.Trim();
                query = query.Where(p =>
                    p.SoPhieu.Contains(term) ||
                    p.NguoiNopNhan.Contains(term) ||
                    (p.LyDoChung != null && p.LyDoChung.Contains(term)));
            }

            // Filter 5: Amount Range
            decimal minVal;
            if (!string.IsNullOrWhiteSpace(MinAmountText) && decimal.TryParse(MinAmountText, out minVal))
                query = query.Where(p => p.TongTien >= minVal);

            decimal maxVal;
            if (!string.IsNullOrWhiteSpace(MaxAmountText) && decimal.TryParse(MaxAmountText, out maxVal))
                query = query.Where(p => p.TongTien <= maxVal);

            PhieuThuChis = new ObservableCollection<PhieuThuChi>(query.OrderBy(p => p.NgayLap).ToList());
        }

        private void ClearFilters()
        {
            var now = DateTime.Now;
            StartDate = new DateTime(now.Year, now.Month, 1);
            EndDate = StartDate.Value.AddMonths(1).AddDays(-1);
            SelectedLoaiPhieu = "Tất cả";
            SelectedMaQuy = null;
            SelectedMaDM = null;
            SearchText = null;
            MinAmountText = null;
            MaxAmountText = null;

            LoadData();
        }
    }
}
