using Expense_Tracker.Module;
using Expense_Tracker.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Expense_Tracker.ViewModel
{
    /// <summary>
    /// Wrapper for displaying budget line items with actual expense comparison.
    /// </summary>
    public class BudgetLineItem : BaseViewModel
    {
        public int MaChiTietNganSach { get; set; }

        private string _maDM;
        public string MaDM
        {
            get => _maDM;
            set { _maDM = value; OnPropertyChanged(); OnPropertyChanged(nameof(TenDM)); }
        }

        public string TenDM { get; set; }

        private decimal _soTienKeHoach;
        public decimal SoTienKeHoach
        {
            get => _soTienKeHoach;
            set
            {
                _soTienKeHoach = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TiLe));
            }
        }

        private decimal _thucTe;
        public decimal ThucTe
        {
            get => _thucTe;
            set { _thucTe = value; OnPropertyChanged(); OnPropertyChanged(nameof(TiLe)); }
        }

        /// <summary>Percentage of actual vs planned (0–100+).</summary>
        public double TiLe => SoTienKeHoach > 0 ? (double)(ThucTe / SoTienKeHoach * 100) : 0;
    }

    public class BudgetViewModel : BaseViewModel
    {
        private EXPENSE_TRACKER_DB_Entities _context;

        // ── Month/Year selection ──
        public ObservableCollection<int> Months { get; set; }
            = new ObservableCollection<int>(Enumerable.Range(1, 12));

        private int _selectedMonth;
        public int SelectedMonth
        {
            get => _selectedMonth;
            set { _selectedMonth = value; OnPropertyChanged(); }
        }

        private int _selectedYear;
        public int SelectedYear
        {
            get => _selectedYear;
            set { _selectedYear = value; OnPropertyChanged(); }
        }

        // ── Budget lines ──
        private ObservableCollection<BudgetLineItem> _budgetLines;
        public ObservableCollection<BudgetLineItem> BudgetLines
        {
            get => _budgetLines;
            set { _budgetLines = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalPlanned)); OnPropertyChanged(nameof(TotalActual)); }
        }

        // ── Lookup ──
        public ObservableCollection<DanhMuc> DanhMucs { get; set; }

        // ── Summary ──
        public decimal TotalPlanned => BudgetLines?.Sum(x => x.SoTienKeHoach) ?? 0;
        public decimal TotalActual => BudgetLines?.Sum(x => x.ThucTe) ?? 0;

        // ── Commands ──
        public ICommand LoadBudgetCommand { get; }
        public ICommand SaveBudgetCommand { get; }
        public ICommand AddLineCommand { get; }
        public ICommand DeleteLineCommand { get; }

        // ── Internal tracking ──
        private NganSach _currentNganSach;

        public BudgetViewModel()
        {
            _context = new EXPENSE_TRACKER_DB_Entities();

            SelectedMonth = DateTime.Now.Month;
            SelectedYear = DateTime.Now.Year;

            DanhMucs = new ObservableCollection<DanhMuc>(
                _context.DanhMuc.Where(d => d.LoaiDM == "Chi").ToList());

            BudgetLines = new ObservableCollection<BudgetLineItem>();

            LoadBudgetCommand = new RelayCommand(o => LoadBudget());
            SaveBudgetCommand = new RelayCommand(o => SaveBudget());

            AddLineCommand = new RelayCommand(o =>
            {
                BudgetLines.Add(new BudgetLineItem
                {
                    MaDM = "",
                    SoTienKeHoach = 0,
                    ThucTe = 0
                });
                OnPropertyChanged(nameof(TotalPlanned));
            });

            DeleteLineCommand = new RelayCommand(param =>
            {
                if (param is BudgetLineItem line)
                {
                    // Remove from DB if persisted
                    if (line.MaChiTietNganSach > 0)
                    {
                        var entity = _context.ChiTietNganSach.Find(line.MaChiTietNganSach);
                        if (entity != null)
                            _context.ChiTietNganSach.Remove(entity);
                    }
                    BudgetLines.Remove(line);
                    OnPropertyChanged(nameof(TotalPlanned));
                    OnPropertyChanged(nameof(TotalActual));
                }
            });

            LoadBudget();
        }

        private void LoadBudget()
        {
            int maND = SessionManager.CurrentUser?.MaND ?? 1;

            // Find existing budget for this month/year/user
            _currentNganSach = _context.NganSach
                .FirstOrDefault(ns => ns.Thang == SelectedMonth
                                   && ns.Nam == SelectedYear
                                   && ns.MaND == maND);

            var lines = new ObservableCollection<BudgetLineItem>();

            if (_currentNganSach != null)
            {
                var details = _context.ChiTietNganSach
                    .Include("DanhMuc")
                    .Where(ct => ct.MaNganSach == _currentNganSach.MaNganSach)
                    .ToList();

                foreach (var ct in details)
                {
                    decimal actual = GetActualExpense(ct.MaDM);
                    lines.Add(new BudgetLineItem
                    {
                        MaChiTietNganSach = ct.MaChiTietNganSach,
                        MaDM = ct.MaDM,
                        TenDM = ct.DanhMuc?.TenDM ?? ct.MaDM,
                        SoTienKeHoach = ct.SoTienKeHoach,
                        ThucTe = actual
                    });
                }
            }
            else
            {
                // Auto-populate all categories with 0 plan if no budget exists
                foreach (var dm in DanhMucs)
                {
                    lines.Add(new BudgetLineItem
                    {
                        MaDM = dm.MaDM,
                        TenDM = dm.TenDM,
                        SoTienKeHoach = 0,
                        ThucTe = GetActualExpense(dm.MaDM)
                    });
                }
            }

            BudgetLines = lines;
        }

        private decimal GetActualExpense(string maDM)
        {
            if (string.IsNullOrEmpty(maDM)) return 0;

            var result = _context.ChiTietPhieu
                .Where(ct => ct.MaDM == maDM
                    && ct.PhieuThuChi.LoaiPhieu == "Chi"
                    && ct.PhieuThuChi.NgayLap.Month == SelectedMonth
                    && ct.PhieuThuChi.NgayLap.Year == SelectedYear)
                .Sum(ct => (decimal?)ct.SoTien);

            return result ?? 0;
        }

        private void SaveBudget()
        {
            try
            {
                // Validate duplicate categories
                var duplicateCategories = BudgetLines
                    .Where(l => !string.IsNullOrEmpty(l.MaDM))
                    .GroupBy(l => l.MaDM)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.First().TenDM)
                    .ToList();

                if (duplicateCategories.Any())
                {
                    MessageBox.Show("Các hạng mục sau bị trùng lặp: " + string.Join(", ", duplicateCategories) + 
                                    "\nMột hạng mục chỉ được thêm một lần trong ngân sách tháng.", 
                                    "Lỗi dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int maND = SessionManager.CurrentUser?.MaND ?? 1;

                // Create NganSach header if not exists
                if (_currentNganSach == null)
                {
                    _currentNganSach = new NganSach
                    {
                        MaND = maND,
                        Thang = SelectedMonth,
                        Nam = SelectedYear,
                        TongTien = 0
                    };
                    _context.NganSach.Add(_currentNganSach);
                    _context.SaveChanges(); // Get MaNganSach generated
                }

                // Update header total
                _currentNganSach.TongTien = BudgetLines.Sum(x => x.SoTienKeHoach);

                // Sync detail lines
                foreach (var line in BudgetLines)
                {
                    if (string.IsNullOrEmpty(line.MaDM)) continue;

                    if (line.MaChiTietNganSach > 0)
                    {
                        // Update existing
                        var entity = _context.ChiTietNganSach.Find(line.MaChiTietNganSach);
                        if (entity != null)
                        {
                            entity.MaDM = line.MaDM;
                            entity.SoTienKeHoach = line.SoTienKeHoach;
                        }
                    }
                    else
                    {
                        // Add new
                        var newDetail = new ChiTietNganSach
                        {
                            MaNganSach = _currentNganSach.MaNganSach,
                            MaDM = line.MaDM,
                            SoTienKeHoach = line.SoTienKeHoach
                        };
                        _context.ChiTietNganSach.Add(newDetail);
                    }
                }

                _context.SaveChanges();
                LoadBudget(); // Refresh to get generated IDs and recalculate actuals

                MessageBox.Show("Lưu ngân sách thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                string errorMsg = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMsg += "\nChi tiết: " + ex.InnerException.Message;
                    if (ex.InnerException.InnerException != null)
                        errorMsg += "\n" + ex.InnerException.InnerException.Message;
                }
                MessageBox.Show("Lỗi khi lưu ngân sách: " + errorMsg, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
