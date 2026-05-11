using Expense_Tracker.Module;
using Expense_Tracker.View;
using System.Windows;

namespace Expense_Tracker
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            bool isAppRunning = true;

            while (isAppRunning)
            {
                var loginView = new LoginView();
                bool? loginResult = loginView.ShowDialog();

                if (loginResult == true && SessionManager.CurrentUser != null)
                {
                    var mainWindow = new MainWindow();

                    Application.Current.MainWindow = mainWindow;
                    mainWindow.ShowDialog();
                    if (SessionManager.CurrentUser == null)
                    {
                        continue;
                    }
                    else
                    {
                        isAppRunning = false;
                    }
                }
                else
                {
                    isAppRunning = false;
                }
            }
            Application.Current.Shutdown();
        }
    }
}