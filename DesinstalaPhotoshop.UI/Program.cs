namespace DesinstalaPhotoshop.UI
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Configuración de la aplicación
            ApplicationConfiguration.Initialize();

            // Aplicar tema oscuro
            Application.SetColorMode(SystemColorMode.Dark);

            // Iniciar la aplicación
            Application.Run(new MainForm());
        }
    }
}