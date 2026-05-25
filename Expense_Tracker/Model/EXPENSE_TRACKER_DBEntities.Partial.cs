using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Expense_Tracker.Model
{
    /// <summary>
    /// Partial class mở rộng EXPENSE_TRACKER_DBEntities để thêm wrapper
    /// cho các Stored Procedure chưa được ánh xạ qua FunctionImport trong EDMX,
    /// mà không động chạm vào các file auto-generated (Model1.Context.cs).
    /// </summary>
    public partial class EXPENSE_TRACKER_DBEntities
    {
        /// <summary>
        /// Gọi sp_TongHopChiTieuTheoDanhMuc để tổng hợp chi tiêu theo danh mục
        /// trong khoảng thời gian cho trước.
        /// </summary>
        /// <param name="loaiDM">Loại danh mục (ví dụ: "Chi"). Không được null.</param>
        /// <param name="tuNgay">Ngày bắt đầu (nullable — NULL nghĩa là không lọc từ ngày).</param>
        /// <param name="denNgay">Ngày kết thúc (nullable — NULL nghĩa là không lọc đến ngày).</param>
        /// <returns>Danh sách kết quả theo thứ tự TongTien giảm dần.</returns>
        public IEnumerable<sp_TongHopChiTieuTheoDanhMuc_Result> sp_TongHopChiTieuTheoDanhMuc(
            string loaiDM,
            DateTime? tuNgay,
            DateTime? denNgay)
        {
            var p0 = new SqlParameter("@LoaiDM", System.Data.SqlDbType.NVarChar, 10)
            {
                Value = loaiDM ?? (object)DBNull.Value
            };

            var p1 = new SqlParameter("@TuNgay", System.Data.SqlDbType.DateTime)
            {
                Value = tuNgay.HasValue ? (object)tuNgay.Value : DBNull.Value
            };

            var p2 = new SqlParameter("@DenNgay", System.Data.SqlDbType.DateTime)
            {
                Value = denNgay.HasValue ? (object)denNgay.Value : DBNull.Value
            };

            return Database.SqlQuery<sp_TongHopChiTieuTheoDanhMuc_Result>(
                "EXEC dbo.sp_TongHopChiTieuTheoDanhMuc @LoaiDM, @TuNgay, @DenNgay",
                p0, p1, p2);
        }
    }
}
