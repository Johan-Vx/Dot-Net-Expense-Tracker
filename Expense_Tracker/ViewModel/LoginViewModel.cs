    using Expense_Tracker.Module;
using Expense_Tracker.Model;
using System.Linq;
using System.Windows.Input;
using System;
using System.Windows.Controls;
using System.Security.Cryptography;
using System.Text;

namespace Expense_Tracker.ViewModel
{
    public class LoginViewModel : BaseViewModel
    {
        private string _username;
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        // Thêm thuộc tính này để Bind với Window
        private bool? _dialogResult;
        public bool? DialogResult
        {
            get => _dialogResult;
            set
            {
                _dialogResult = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoginCommand { get; }
        public ICommand CloseCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(ExecuteLogin);
            CloseCommand = new RelayCommand(o => DialogResult = false); // Đóng form (Cancel)
        }

        private void ExecuteLogin(object parameter)
        {
            var passwordBox = parameter as PasswordBox;
            if (passwordBox == null) return;

            string password = passwordBox.Password;

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(password))
            {
                ErrorMessage = "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.";
                return;
            }

            try
            {
                string hash = HashPassword(password);
                using (var context = new EXPENSE_TRACKER_DBEntities())
                {
                    var user = context.NguoiDung.FirstOrDefault(u => u.TenDangNhap == Username && u.MatKhauHash == hash);
                    if (user != null)
                    {
                        SessionManager.CurrentUser = user;
                        DialogResult = true; // Đăng nhập thành công -> tự động đóng và trả về true
                    }
                    else
                    {
                        ErrorMessage = "Tên đăng nhập hoặc mật khẩu không đúng.";
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Lỗi kết nối cơ sở dữ liệu: " + ex.Message;
            }
        }

        private string HashPassword(string input)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}