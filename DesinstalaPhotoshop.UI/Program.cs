using System;
using System.Linq;
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
            Application.SetColorMode(SystemColorMode.Dark);

            // Verificar si la aplicación se inició con los argumentos de elevación
            bool isElevatedForGeneral = args.Contains("--elevated");
            bool isElevatedForDetection = args.Contains("--elevated-for-detection");

            // Iniciar la aplicación pasando los parámetros de elevación
            Application.Run(new MainForm(
                isElevated: isElevatedForGeneral || isElevatedForDetection, 
                justElevatedForDetection: isElevatedForDetection
            ));
        }
    }
}