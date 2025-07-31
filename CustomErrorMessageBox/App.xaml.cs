using CustomErrorMessageBox.MVVM.Views.ErrorMessageBox;
using System.Configuration;
using System.Data;
using System.Windows;

namespace CustomErrorMessageBox
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            MainWindow = new BaseErrorMessageBox();
            MainWindow.Show();
            base.OnStartup(e);
        }
    }
}
