using Expense_Tracker.Module;
using Expense_Tracker.Model;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Expense_Tracker.ViewModel
{
    public class SettingsViewModel : BaseViewModel
    {
        private EXPENSE_TRACKER_DBEntities _context;

        public ObservableCollection<DanhMuc> DanhMucs { get; set; }
        public ObservableCollection<TaiKhoanQuy> TaiKhoanQuys { get; set; }
        public ObservableCollection<NguoiDung> NguoiDungs { get; set; }

        public ICommand SaveCommand { get; }
        public ICommand RefreshCommand { get; }

        public SettingsViewModel()
        {
            _context = new EXPENSE_TRACKER_DBEntities();
            
            SaveCommand = new RelayCommand(o => {
                _context.SaveChanges();
            });

            RefreshCommand = new RelayCommand(o => LoadData());

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
