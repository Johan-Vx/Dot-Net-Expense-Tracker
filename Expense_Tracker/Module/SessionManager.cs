using System;
using Expense_Tracker.Model;

namespace Expense_Tracker.Module
{
    public static class SessionManager
    {
        public static NguoiDung CurrentUser { get; set; }
        public static bool IsAdmin => CurrentUser != null && CurrentUser.MaVaiTro == 1;
    }
}
