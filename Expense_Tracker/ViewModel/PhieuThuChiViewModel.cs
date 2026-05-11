using Expense_Tracker.Model;
using Expense_Tracker.Module;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data.Entity.Validation;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Expense_Tracker.ViewModel
{
    public class PhieuThuChiViewModel : BaseViewModel
    {
        private EXPENSE_TRACKER_DBEntities _context;

        // ── Master list ──────────────────────────────────────────────
        public ObservableCollection<PhieuThuChi> PhieuThuChis { get; set; }

        private PhieuThuChi _selectedPhieu;
        public PhieuThuChi SelectedPhieu
        {
            get => _selectedPhieu;
            set
            {
                _selectedPhieu = value;
                OnPropertyChanged();
                // Reload filtered DanhMuc when LoaiPhieu may differ
                OnPropertyChanged(nameof(FilteredDanhMucs));

                if (_selectedPhieu != null && !string.IsNullOrEmpty(_selectedPhieu.SoPhieu))
                    LoadChiTiet();
                else
                    ResetChiTiet();
            }
        }

        // ── ChiTiet collection ───────────────────────────────────────
        private ObservableCollection<ChiTietPhieu> _chiTietPhieus;
        public ObservableCollection<ChiTietPhieu> ChiTietPhieus
        {
            get => _chiTietPhieus;
            set
            {
                // Unsubscribe old
                if (_chiTietPhieus != null)
                    _chiTietPhieus.CollectionChanged -= OnChiTietChanged;

                _chiTietPhieus = value;
                OnPropertyChanged();

                // Subscribe new
                if (_chiTietPhieus != null)
                    _chiTietPhieus.CollectionChanged += OnChiTietChanged;

                RecalcTongTien();
            }
        }

        /// <summary>Real-time sum: fires whenever a row is added/removed.</summary>
        private void OnChiTietChanged(object sender, NotifyCollectionChangedEventArgs e)
            => RecalcTongTien();

        private void RecalcTongTien()
        {
            if (SelectedPhieu == null || ChiTietPhieus == null) return;
            SelectedPhieu.TongTien = ChiTietPhieus.Sum(x => x.SoTien);
            OnPropertyChanged(nameof(SelectedPhieu));
        }

        // ── Lookup collections ───────────────────────────────────────
        public ObservableCollection<string>      LoaiPhieus   { get; set; }
            = new ObservableCollection<string> { "Thu", "Chi" };
        public ObservableCollection<DanhMuc>     DanhMucs     { get; set; }
        public ObservableCollection<TaiKhoanQuy> TaiKhoanQuys { get; set; }

        /// <summary>
        /// DanhMuc filtered by the current phieu's LoaiPhieu (Thu/Chi).
        /// Matches DB constraint: DanhMuc.LoaiDM IN ('Thu','Chi').
        /// </summary>
        public ObservableCollection<DanhMuc> FilteredDanhMucs
        {
            get
            {
                if (SelectedPhieu == null || DanhMucs == null)
                    return DanhMucs;

                var filtered = DanhMucs
                    .Where(d => d.LoaiDM == SelectedPhieu.LoaiPhieu)
                    .ToList();

                return new ObservableCollection<DanhMuc>(filtered);
            }
        }

        // ── Commands ─────────────────────────────────────────────────
        public ICommand AddCommand          { get; }
        public ICommand SaveCommand         { get; }
        public ICommand DeleteCommand       { get; }
        public ICommand AddChiTietCommand   { get; }
        public ICommand DeleteChiTietCommand { get; }

        public PhieuThuChiViewModel()
        {
            _context = new EXPENSE_TRACKER_DBEntities();
            LoadData();

            // ── Thêm phiếu mới ──
            AddCommand = new RelayCommand(_ =>
            {
                var newPhieu = _context.PhieuThuChi.Create();
                newPhieu.NgayLap        = DateTime.Now;
                newPhieu.NguoiLapPhieu  = SessionManager.CurrentUser?.MaND ?? 1;
                newPhieu.LoaiPhieu      = "Chi";
                newPhieu.NguoiNopNhan   = "";
                newPhieu.TongTien       = 0;

                PhieuThuChis.Add(newPhieu);
                SelectedPhieu = newPhieu;
            });

            // ── Lưu phiếu ──
            SaveCommand = new RelayCommand(_ =>
            {
                if (SelectedPhieu == null) return;

                // Validate #1 — NguoiNopNhan bắt buộc (DB NOT NULL)
                if (string.IsNullOrWhiteSpace(SelectedPhieu.NguoiNopNhan))
                {
                    MessageBox.Show("Vui lòng nhập Người Nộp / Nhận.",
                        "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Validate #2 — MaQuy bắt buộc (DB FK NOT NULL)
                if (string.IsNullOrEmpty(SelectedPhieu.MaQuy))
                {
                    MessageBox.Show("Vui lòng chọn Tài Khoản Quỹ.",
                        "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Validate #3 — MaDM bắt buộc trên mỗi dòng ChiTiet (DB FK NOT NULL)
                if (ChiTietPhieus != null &&
                    ChiTietPhieus.Any(x => string.IsNullOrEmpty(x.MaDM)))
                {
                    MessageBox.Show("Vui lòng chọn Danh Mục cho tất cả các dòng chi tiết.",
                        "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Validate #4 — SoTien > 0 (DB CHECK constraint)
                if (ChiTietPhieus != null &&
                    ChiTietPhieus.Any(x => x.SoTien <= 0))
                {
                    MessageBox.Show("Số tiền mỗi dòng chi tiết phải lớn hơn 0.",
                        "Giá trị không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Sinh SoPhieu nếu phiếu mới (max 20 ký tự — VARCHAR(20))
                if (string.IsNullOrEmpty(SelectedPhieu.SoPhieu))
                {
                    SelectedPhieu.SoPhieu = "PTC" + DateTime.Now.ToString("yyMMddHHmmssff");
                    _context.PhieuThuChi.Add(SelectedPhieu);

                    // Gán SoPhieu cho các dòng ChiTiet đang chờ
                    if (ChiTietPhieus != null)
                        foreach (var ct in ChiTietPhieus.Where(c => string.IsNullOrEmpty(c.SoPhieu)))
                        {
                            ct.SoPhieu = SelectedPhieu.SoPhieu;
                            _context.ChiTietPhieu.Add(ct);
                        }
                }

                // Tổng tiền tự tính (real-time đã cập nhật, lưu lại chắc chắn)
                SelectedPhieu.TongTien = ChiTietPhieus?.Sum(x => x.SoTien) ?? 0;

                try
                {
                    _context.SaveChanges();
                    LoadData();
                }
                catch (DbEntityValidationException ex)
                {
                    var msgs = ex.EntityValidationErrors
                        .SelectMany(e => e.ValidationErrors)
                        .Select(ve => $"• {ve.PropertyName}: {ve.ErrorMessage}");

                    MessageBox.Show(
                        "Lỗi lưu dữ liệu:\n" + string.Join("\n", msgs),
                        "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi không xác định: " + ex.Message,
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            // ── Xóa phiếu ──
            DeleteCommand = new RelayCommand(_ =>
            {
                if (SelectedPhieu == null) return;

                if (!string.IsNullOrEmpty(SelectedPhieu.SoPhieu))
                {
                    var confirm = MessageBox.Show(
                        $"Xóa phiếu {SelectedPhieu.SoPhieu}? Thao tác không thể hoàn tác.",
                        "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (confirm != MessageBoxResult.Yes) return;

                    _context.PhieuThuChi.Remove(SelectedPhieu);
                    _context.SaveChanges();
                }
                else
                {
                    // Phiếu chưa lưu — chỉ xóa khỏi danh sách
                    PhieuThuChis.Remove(SelectedPhieu);
                }

                LoadData();
            });

            // ── Thêm dòng ChiTiet ──
            AddChiTietCommand = new RelayCommand(_ =>
            {
                if (SelectedPhieu == null) return;

                if (ChiTietPhieus == null)
                    ChiTietPhieus = new ObservableCollection<ChiTietPhieu>();

                ChiTietPhieus.Add(new ChiTietPhieu
                {
                    SoPhieu = SelectedPhieu.SoPhieu ?? "",
                    SoTien  = 0,
                    GhiChu  = ""
                });
            });

            // ── Xóa dòng ChiTiet được chọn ──
            DeleteChiTietCommand = new RelayCommand(param =>
            {
                if (param is ChiTietPhieu ct)
                {
                    // Nếu đã có MaCT thì xóa khỏi DB
                    if (ct.MaCT > 0)
                        _context.ChiTietPhieu.Remove(ct);

                    ChiTietPhieus?.Remove(ct);
                    RecalcTongTien();
                }
            });
        }

        // ── Helpers ──────────────────────────────────────────────────
        private void LoadData()
        {
            PhieuThuChis = new ObservableCollection<PhieuThuChi>(
                _context.PhieuThuChi.Include("TaiKhoanQuy").OrderByDescending(p => p.NgayLap).ToList());

            DanhMucs     = new ObservableCollection<DanhMuc>(_context.DanhMuc.ToList());
            TaiKhoanQuys = new ObservableCollection<TaiKhoanQuy>(_context.TaiKhoanQuy.ToList());

            OnPropertyChanged(nameof(PhieuThuChis));
            OnPropertyChanged(nameof(DanhMucs));
            OnPropertyChanged(nameof(TaiKhoanQuys));
            OnPropertyChanged(nameof(FilteredDanhMucs));
        }

        private void LoadChiTiet()
        {
            if (SelectedPhieu == null) return;
            ChiTietPhieus = new ObservableCollection<ChiTietPhieu>(
                _context.ChiTietPhieu
                    .Where(c => c.SoPhieu == SelectedPhieu.SoPhieu)
                    .ToList());
        }

        private void ResetChiTiet()
        {
            ChiTietPhieus = new ObservableCollection<ChiTietPhieu>();
        }
    }
}
