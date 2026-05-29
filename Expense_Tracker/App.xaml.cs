using Expense_Tracker.Module;
using Expense_Tracker.View;
using System;
using System.Windows;

namespace Expense_Tracker
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            DotNetEnv.Env.Load();
            string envConnectionString = Environment.GetEnvironmentVariable("EXPENSE_TRACKER_DBEntities");
            
            string db_user = Environment.GetEnvironmentVariable("DB_USERNAME");
            string db_password = Environment.GetEnvironmentVariable("DB_PASSWORD");
            string db_servername = Environment.GetEnvironmentVariable("DB_SEVERNAME");
            string db_dbname = Environment.GetEnvironmentVariable("DB_DBNAME");

            if (string.IsNullOrEmpty(envConnectionString) && !string.IsNullOrEmpty(db_servername))
            {
                envConnectionString = $@"metadata=res://*/Model.EXPENSE_TRACKER_DB.csdl|res://*/Model.EXPENSE_TRACKER_DB.ssdl|res://*/Model.EXPENSE_TRACKER_DB.msl;provider=System.Data.SqlClient;provider connection string=""data source={db_servername};initial catalog={db_dbname};user id={db_user};password={db_password};trustservercertificate=True;MultipleActiveResultSets=True;App=EntityFramework""";
            }

            if (!string.IsNullOrEmpty(envConnectionString))
            {
                string configPath = AppDomain.CurrentDomain.BaseDirectory + "connections.config";
                string xmlContent = $@"<connectionStrings>
    <add name=""EXPENSE_TRACKER_DBEntities"" connectionString=""{envConnectionString}"" providerName=""System.Data.EntityClient"" />
</connectionStrings>";
                System.IO.File.WriteAllText(configPath, xmlContent);
            }
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