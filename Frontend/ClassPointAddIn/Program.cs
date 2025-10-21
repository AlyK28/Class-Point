using System;
using System.Windows.Forms;
using ClassPointAddIn.Views;

namespace ClassPointAddIn
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// This is for testing the C# frontend functionality.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Show test form
            Application.Run(new TestForm());
        }
    }
}
