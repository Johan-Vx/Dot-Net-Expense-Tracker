using Expense_Tracker.Model;
using Expense_Tracker.Module;
using System;
using System.Collections.ObjectModel;
using System.Data.Entity.Validation;
using System.Linq;
using System.Windows.Input;

namespace Expense_Tracker.ViewModel
{
    public class PhieuThuChiViewModel : BaseViewModel
    {
        private EXPENSE_TRACKER_DBEntities _context;

        public ObservableCollection<PhieuThuChi> PhieuThuChis { get; set; }
        
        private PhieuThuChi _selectedPhieu;
        public PhieuThuChi SelectedPhieu
        {
            get => _selectedPhieu;
            set
            {
                _selectedPhieu = value;
                OnPropertyChanged();
                if (_selectedPhieu != null && !string.IsNullOrEmpty(_selectedPhieu.SoPhieu))
                {
                    LoadChiTiet();
                }
                else
                {
                    ChiTietPhieus = new ObservableCollection<ChiTietPhieu>();
                }
            }
        }

        private ObservableCollection<ChiTietPhieu> _chiTietPhieus;
        public ObservableCollection<ChiTietPhieu> ChiTietPhieus
        {
            get => _chiTietPhieus;
            set { _chiTietPhieus = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> LoaiPhieus { get; set; } = new ObservableCollection<string> { "Thu", "Chi" };
        public ObservableCollection<DanhMuc> DanhMucs { get; set; }
        public ObservableCollection<TaiKhoanQuy> TaiKhoanQuys { get; set; }

        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }

        public PhieuThuChiViewModel()
        {
            _context = new EXPENSE_TRACKER_DBEntities();
            LoadData();

            AddCommand = new RelayCommand(o => 
            {
                var newPhieu = _context.PhieuThuChi.Create();
                newPhieu.NgayLap = DateTime.Now;
                newPhieu.NguoiLapPhieu = SessionManager.CurrentUser?.MaND ?? 1;
                newPhieu.LoaiPhieu = "Chi";
                newPhieu.NguoiNopNhan = "";
                
                PhieuThuChis.Add(newPhieu);
                SelectedPhieu = newPhieu;
            });

            SaveCommand = new RelayCommand(o => 
            {
                if (SelectedPhieu == null) return;
                
                if (string.IsNullOrWhiteSpace(SelectedPhieu.NguoiNopNhan))
                {
                    System.Windows.MessageBox.Show("Vui lòng nhập người nộp/nhận.");
                    return;
                }
                
                if (SelectedPhieu.TaiKhoanQuy == null && string.IsNullOrEmpty(SelectedPhieu.MaQuy))
                {
                    System.Windows.MessageBox.Show("Vui lòng chọn tài khoản quỹ.");
                    return;
                }
                
                if (string.IsNullOrEmpty(SelectedPhieu.SoPhieu))
                {
                    SelectedPhieu.SoPhieu = "PTC" + DateTime.Now.ToString("yyMMddHHmmssff");
                    _context.PhieuThuChi.Add(SelectedPhieu);
                }
                
                // Recalculate SoTien
                SelectedPhieu.TongTien = ChiTietPhieus.Sum(x => x.SoTien);

                try
                {
                    _context.SaveChanges();
                 LoadData();
                }
                catch (DbEntityValidationException e)
                            {
                    foreach (var eve in e.EntityValidationErrors)
                            {
                                Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                    eve.Entry.Entity.GetType().Name, eve.Entry.State);
                                foreach (var ve in eve.ValidationErrors)
                                {
                                    Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                        ve.PropertyName, ve.ErrorMessage);
                                }
                            }
                            throw;
                        }
                    });

            DeleteCommand = new RelayCommand(o => 
            {
                if (SelectedPhieu != null && !string.IsNullOrEmpty(SelectedPhieu.SoPhieu))
                {
                    _context.PhieuThuChi.Remove(SelectedPhieu);
                    _context.SaveChanges();
                    LoadData();
                }
            });
        }

        private void LoadData()
        {
            PhieuThuChis = new ObservableCollection<PhieuThuChi>(_context.PhieuThuChi.Include("TaiKhoanQuy").ToList());
            DanhMucs = new ObservableCollection<DanhMuc>(_context.DanhMuc.ToList());
            TaiKhoanQuys = new ObservableCollection<TaiKhoanQuy>(_context.TaiKhoanQuy.ToList());
            
            OnPropertyChanged(nameof(PhieuThuChis));
            OnPropertyChanged(nameof(DanhMucs));
            OnPropertyChanged(nameof(TaiKhoanQuys));
        }

        private void LoadChiTiet()
        {
            if (SelectedPhieu == null) return;
            ChiTietPhieus = new ObservableCollection<ChiTietPhieu>(_context.ChiTietPhieu.Where(c => c.SoPhieu == SelectedPhieu.SoPhieu).ToList());
        }
    }
}
