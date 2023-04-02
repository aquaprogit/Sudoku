using System.Windows;

namespace Sudoku
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow window = new MainWindow();
            window.Show();
        }
    }
}