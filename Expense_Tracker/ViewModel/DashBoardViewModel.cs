using Expense_Tracker.Module;
using Expense_Tracker.Model;
using System.Linq;
using System.Collections.ObjectModel;
using System;
using System.Data.SqlClient;

namespace Expense_Tracker.ViewModel
{
    public class DashBoardViewModel : BaseViewModel
    {
        private decimal _tongSoDu;
        public decimal TongSoDu
        {
            get => _tongSoDu;
            set { _tongSoDu = value; OnPropertyChanged(); }
        }

        private decimal _tongThuThangNay;
        public decimal TongThuThangNay
        {
            get => _tongThuThangNay;
            set { _tongThuThangNay = value; OnPropertyChanged(); }
        }

        private decimal _tongChiThangNay;
        public decimal TongChiThangNay
        {
            get => _tongChiThangNay;
            set { _tongChiThangNay = value; OnPropertyChanged(); }
        }

        public ObservableCollection<PhieuThuChi> RecentTransactions { get; set; }

        public DashBoardViewModel()
        {
            LoadData();
        }

        private void LoadData()
        {
            using (var context = new EXPENSE_TRACKER_DBEntities())
            {
                try 
                {
                    TongSoDu = context.Database.SqlQuery<decimal>("SELECT dbo.func_LayTongSoDu()").FirstOrDefault();

                    var now = DateTime.Now;
                    var thongKe = context.sproc_ThongKeThuChiThang(now.Month, now.Year).ToList();

                    TongThuThangNay = thongKe.FirstOrDefault(x => x.LoaiPhieu == "Thu")?.TongTienThang ?? 0m;
                    TongChiThangNay = thongKe.FirstOrDefault(x => x.LoaiPhieu == "Chi")?.TongTienThang ?? 0m;

                    var recent = context.PhieuThuChi.Include("TaiKhoanQuy").Include("NguoiDung")
                                        .OrderByDescending(p => p.NgayLap)
                                        .Take(10).ToList();
                    
                    RecentTransactions = new ObservableCollection<PhieuThuChi>(recent);
                }
                catch
                {
                    RecentTransactions = new ObservableCollection<PhieuThuChi>();
                }
            }
        }
    }


}
