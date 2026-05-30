using Expense_Tracker.Module;
using Expense_Tracker.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Expense_Tracker.ViewModel
{
    public class SettingsViewModel : BaseViewModel
    {
        private EXPENSE_TRACKER_DB_Entities _context;

        public ObservableCollection<DanhMuc> DanhMucs { get; set; }
        public ObservableCollection<TaiKhoanQuy> TaiKhoanQuys { get; set; }
        public ObservableCollection<NguoiDung> NguoiDungs { get; set; }

        private TaiKhoanQuy _selectedFund;
        public TaiKhoanQuy SelectedFund
        {
            get => _selectedFund;
            set { _selectedFund = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand AddFundCommand { get; }
        public ICommand DeleteFundCommand { get; }

        public SettingsViewModel()
        {
            _context = new EXPENSE_TRACKER_DB_Entities();

            SaveCommand = new RelayCommand(o =>
            {
                try
                {
                    _context.SaveChanges();
                    MessageBox.Show("Lưu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi lưu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            RefreshCommand = new RelayCommand(o => LoadData());

            // ── Add Fund ──
            AddFundCommand = new RelayCommand(o =>
            {
                var newFund = new TaiKhoanQuy
                {
                    MaQuy = "QUY" + DateTime.Now.ToString("yyMMddHHmmss"),
                    TenQuy = "Quỹ mới",
                    SoDuBanDau = 0,
                    SoDuHienTai = 0
                };

                _context.TaiKhoanQuy.Add(newFund);
                TaiKhoanQuys.Add(newFund);
                SelectedFund = newFund;
            });

            // ── Delete Fund with integrity check ──
            DeleteFundCommand = new RelayCommand(o =>
            {
                if (SelectedFund == null)
                {
                    MessageBox.Show("Vui lòng chọn quỹ cần xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Integrity check: block if fund has vouchers
                bool hasVouchers = _context.PhieuThuChi.Any(p => p.MaQuy == SelectedFund.MaQuy);
                if (hasVouchers)
                {
                    MessageBox.Show(
                        "Không thể xóa quỹ \"" + SelectedFund.TenQuy + "\" vì đã có phiếu thu/chi liên quan.\n" +
                        "Vui lòng xóa hoặc chuyển các phiếu thu/chi sang quỹ khác trước.",
                        "Lỗi ràng buộc dữ liệu", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var confirm = MessageBox.Show(
                    "Xác nhận xóa quỹ \"" + SelectedFund.TenQuy + "\"?\nThao tác không thể hoàn tác.",
                    "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (confirm != MessageBoxResult.Yes) return;

                try
                {
                    _context.TaiKhoanQuy.Remove(SelectedFund);
                    _context.SaveChanges();
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xóa quỹ: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            LoadData();
        }

        private void LoadData()
        {
            DanhMucs = new ObservableCollection<DanhMuc>(_context.DanhMuc.ToList());
            TaiKhoanQuys = new ObservableCollection<TaiKhoanQuy>(_context.TaiKhoanQuy.ToList());
            NguoiDungs = new ObservableCollection<NguoiDung>(_context.NguoiDung.Include("VaiTro").ToList());
            
            OnPropertyChanged(nameof(DanhMucs));
            OnPropertyChanged(nameof(TaiKhoanQuys));
            OnPropertyChanged(nameof(NguoiDungs));
        }
    }
}
