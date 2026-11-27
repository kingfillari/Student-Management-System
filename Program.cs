using System;
using System.Windows.Forms;

namespace StudentManagementSystem;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        try
        {
            Database.Initialize();
            Application.Run(new LoginForm());
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Application startup failed:\n" + ex.Message,
                "Startup Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
