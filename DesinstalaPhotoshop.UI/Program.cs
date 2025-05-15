using System;
using System.Windows.Forms;

namespace DesinstalaPhotoshop.UI
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Configuración de la aplicación
            ApplicationConfiguration.Initialize();

            // Aplicar tema oscuro
            Application.SetColorMode(SystemColorMode.Dark);

            // Verificar si la aplicación se inició con el argumento --elevated
            bool isElevated = Array.Exists(args, arg => arg == "--elevated");

            // Iniciar la aplicación pasando el estado de elevación
            Application.Run(new MainForm(isElevated));
        }
    }
}