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

        private ObservableCollection<PhieuThuChi> _phieuThuChis;
        public ObservableCollection<PhieuThuChi> PhieuThuChis
        {
            get => _phieuThuChis;
            set { _phieuThuChis = value; OnPropertyChanged(); }
        }

        public ICommand FilterCommand { get; }

        public LedgerViewModel()
        {
            _context = new EXPENSE_TRACKER_DBEntities();
            
            // Default filter: current month
            var now = DateTime.Now;
            StartDate = new DateTime(now.Year, now.Month, 1);
            EndDate = StartDate.Value.AddMonths(1).AddDays(-1);

            FilterCommand = new RelayCommand(o => LoadData());

            LoadData();
        }

        private void LoadData()
        {
            var query = _context.PhieuThuChi.Include("TaiKhoanQuy").Include("NguoiDung").AsQueryable();

            if (StartDate.HasValue)
                query = query.Where(p => p.NgayLap >= StartDate.Value);
            
            if (EndDate.HasValue)
            {
                var endOfDay = EndDate.Value.Date.AddDays(1).AddSeconds(-1);
                query = query.Where(p => p.NgayLap <= endOfDay);
            }

            PhieuThuChis = new ObservableCollection<PhieuThuChi>(query.OrderBy(p => p.NgayLap).ToList());
        }
    }
}
