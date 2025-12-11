using System;
using System.Windows.Forms;

namespace FleetApp.UI
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.Run(new Form1());
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            ShowFriendlyError(e.Exception);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                ShowFriendlyError(ex);
            }
            else
            {
                MessageBox.Show("An unexpected error occurred.", "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void ShowFriendlyError(Exception ex)
        {
            string message = GetInnermostMessage(ex);
            MessageBox.Show(message, "Operation Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private static string GetInnermostMessage(Exception ex)
        {
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
            }

            return ex.Message;
        }
    }
}
