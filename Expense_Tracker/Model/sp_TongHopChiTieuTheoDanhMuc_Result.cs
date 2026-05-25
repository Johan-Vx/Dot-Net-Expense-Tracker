namespace Expense_Tracker.Model
{
    /// <summary>
    /// Kết quả trả về của Stored Procedure sp_TongHopChiTieuTheoDanhMuc.
    /// Cột: MaDM, TenDM, TongTien.
    /// </summary>
    public class sp_TongHopChiTieuTheoDanhMuc_Result
    {
        public string MaDM    { get; set; }
        public string TenDM   { get; set; }
        public decimal TongTien { get; set; }
    }
}
