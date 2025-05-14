using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Principal;
using System.Linq;
using System.Text.Json;
using CustomMsgBoxLibrary;
using CustomMsgBoxLibrary.Types;

namespace DesinstalaPhotoshop.UI
{
    public partial class RestoreBackupForm : Form
    {
        /// <summary>
        /// La ruta del backup seleccionado
        /// </summary>
        private string _selectedBackupPath = string.Empty;

        /// <summary>
        /// Obtiene la ruta del backup seleccionado
        /// </summary>
        public string SelectedBackupPath => _selectedBackupPath;

        public RestoreBackupForm()
        {
            InitializeComponent();
            
            if (!IsUserAdministrator())
            {
                CustomMsgBox.Show(
                    prompt: "Se requieren privilegios de administrador para restaurar copias de seguridad.",
                    title: "Permisos insuficientes",
                    buttons: CustomMessageBoxButtons.OK,
                    icon: CustomMessageBoxIcon.Warning,
                    theme: ThemeSettings.DarkTheme
                );
                this.Close();
                return;
            }
            
            SetupForm();
            LoadBackups();
        }
        
        private bool IsUserAdministrator()
        {
            try
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        private void SetupForm()
        {
            // Configurar propiedades del formulario
            this.Text = "Restaurar Copia de Seguridad";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.BackColor = Color.FromArgb(20, 30, 45);
            this.ForeColor = Color.White;
            this.Size = new Size(500, 400);
            this.AcceptButton = btnRestore;
            this.CancelButton = btnCancel;
        }

        private void LoadBackups()
        {
            try
            {
                lstBackups.Items.Clear();

                // Obtener la carpeta de backups
                string backupFolder = Path.Combine(Application.StartupPath, "backups");

                if (!Directory.Exists(backupFolder))
                {
                    lblNoBackups.Visible = true;
                    lstBackups.Visible = false;
                    btnRestore.Enabled = false;
                    return;
                }

                // Buscar archivos de backup
                string[] backupFiles = Directory.GetFiles(backupFolder, "*.zip");

                if (backupFiles.Length == 0)
                {
                    lblNoBackups.Visible = true;
                    lstBackups.Visible = false;
                    btnRestore.Enabled = false;
                    return;
                }

                // Mostrar los backups encontrados
                lblNoBackups.Visible = false;
                lstBackups.Visible = true;

                foreach (string backupFile in backupFiles)
                {
                    FileInfo fileInfo = new FileInfo(backupFile);
                    string fileName = fileInfo.Name;
                    string fileDate = fileInfo.CreationTime.ToString("yyyy-MM-dd HH:mm:ss");
                    string fileSize = (fileInfo.Length / 1024 / 1024).ToString() + " MB";

                    ListViewItem item = new ListViewItem(fileName);
                    item.SubItems.Add(fileDate);
                    item.SubItems.Add(fileSize);
                    item.Tag = backupFile;

                    lstBackups.Items.Add(item);
                }

                // Seleccionar el primer backup por defecto
                if (lstBackups.Items.Count > 0)
                {
                    lstBackups.Items[0].Selected = true;
                    btnRestore.Enabled = true;
                }
                else
                {
                    btnRestore.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                CustomMsgBox.Show(
    prompt: $"Error al cargar los backups: {ex.Message}",
    title: "Error",
    buttons: CustomMessageBoxButtons.OK,
    icon: CustomMessageBoxIcon.Error,
    theme: ThemeSettings.DarkTheme
);
            }
        }

        private void LstBackups_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstBackups.SelectedItems.Count > 0)
            {
                _selectedBackupPath = lstBackups.SelectedItems[0].Tag?.ToString() ?? string.Empty;
                btnRestore.Enabled = true;
            }
            else
            {
                _selectedBackupPath = string.Empty;
                btnRestore.Enabled = false;
            }
        }

        private async void BtnRestore_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(SelectedBackupPath))
            {
                CustomMsgBox.Show(
                    prompt: "Por favor, seleccione un backup para restaurar.",
                    title: "Aviso",
                    buttons: CustomMessageBoxButtons.OK,
                    icon: CustomMessageBoxIcon.Warning,
                    theme: ThemeSettings.DarkTheme
                );
                return;
            }

            // Obtener información del backup seleccionado
            FileInfo backupInfo = new FileInfo(SelectedBackupPath);
            string backupName = backupInfo.Name;
            string backupDate = backupInfo.CreationTime.ToString("dd/MM/yyyy HH:mm:ss");
            string backupSize = (backupInfo.Length / 1024 / 1024).ToString("N2") + " MB";

            // Mostrar diálogo de confirmación con detalles del backup
            var result = CustomMsgBox.Show(
                prompt: $"¿Está seguro que desea restaurar la siguiente copia de seguridad?\n\n" +
                        $"Nombre: {backupName}\n" +
                        $"Fecha: {backupDate}\n" +
                        $"Tamaño: {backupSize}\n\n" +
                        "Esta acción revertirá los archivos a su estado anterior.",
                title: "Confirmar Restauración",
                buttons: CustomMessageBoxButtons.YesNo,
                icon: CustomMessageBoxIcon.Question,
                theme: ThemeSettings.DarkTheme
            );

            if (result == CustomDialogResult.Yes)
            {
                try
                {
                    btnRestore.Enabled = false;
                    btnCancel.Enabled = false;
                    lstBackups.Enabled = false;
                    Cursor = Cursors.WaitCursor;

                    // Crear un directorio temporal para la extracción
                    string tempDir = Path.Combine(Path.GetTempPath(), "DesinstalaPhotoshop_Restore_" + Guid.NewGuid().ToString());
                    Directory.CreateDirectory(tempDir);

                    try
                    {
                        // Extraer el archivo ZIP
                        await Task.Run(() => ZipFile.ExtractToDirectory(SelectedBackupPath, tempDir));

                        // Restaurar los archivos a sus ubicaciones originales
                        string manifestPath = Path.Combine(tempDir, "manifest.json");
                        if (File.Exists(manifestPath))
                        {
                            // Leer y deserializar el archivo manifest.json
                        string manifestContent = await File.ReadAllTextAsync(manifestPath);
                        var backupMetadata = System.Text.Json.JsonSerializer.Deserialize<global::DesinstalaPhotoshop.Core.Models.BackupMetadata>(
                            manifestContent,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                        );

                        if (backupMetadata == null)
                        {
                            throw new InvalidOperationException("El archivo manifest.json está vacío o tiene un formato inválido.");
                        }

                        // Restaurar cada elemento del backup
                        foreach (var item in backupMetadata.Items)
                        {
                            try
                            {
                                string sourceFile = Path.Combine(tempDir, item.BackupPath);
                                string targetPath = item.OriginalPath;

                                // Asegurar que el directorio destino existe
                                string? targetDir = Path.GetDirectoryName(targetPath);
                                if (!string.IsNullOrEmpty(targetDir))
                                {
                                    Directory.CreateDirectory(targetDir);
                                }

                                switch (item.ItemType)
                                {
                                    case DesinstalaPhotoshop.Core.Models.BackupItemType.File:
                                        if (File.Exists(sourceFile))
                                        {
                                            File.Copy(sourceFile, targetPath, true);
                                            item.IsRestored = true;
                                            item.RestoreTime = DateTime.Now;
                                        }
                                        break;

                                    case DesinstalaPhotoshop.Core.Models.BackupItemType.Directory:
                                        if (Directory.Exists(sourceFile))
                                        {
                                            if (Directory.Exists(targetPath))
                                            {
                                                Directory.Delete(targetPath, true);
                                            }
                                            Directory.Move(sourceFile, targetPath);
                                            item.IsRestored = true;
                                            item.RestoreTime = DateTime.Now;
                                        }
                                        break;

                                    case DesinstalaPhotoshop.Core.Models.BackupItemType.RegistryKey:
                                        if (File.Exists(sourceFile))
                                        {
                                            // Restaurar clave de registro usando reg.exe
                                            var process = new System.Diagnostics.Process
                                            {
                                                StartInfo = new System.Diagnostics.ProcessStartInfo
                                                {
                                                    FileName = "reg.exe",
                                                    Arguments = $"import \"{sourceFile}\"",
                                                    UseShellExecute = false,
                                                    RedirectStandardOutput = true,
                                                    RedirectStandardError = true,
                                                    CreateNoWindow = true
                                                }
                                            };

                                            process.Start();
                                            await process.WaitForExitAsync();

                                            if (process.ExitCode == 0)
                                            {
                                                item.IsRestored = true;
                                                item.RestoreTime = DateTime.Now;
                                            }
                                            else
                                            {
                                                string error = await process.StandardError.ReadToEndAsync();
                                                item.RestoreErrorMessage = $"Error al restaurar la clave de registro: {error}";
                                            }
                                        }
                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                item.RestoreErrorMessage = ex.Message;
                            }
                        }

                        // Actualizar el manifest.json con el estado de restauración
                        await File.WriteAllTextAsync(
                            manifestPath,
                            System.Text.Json.JsonSerializer.Serialize(backupMetadata, new System.Text.Json.JsonSerializerOptions
                            {
                                WriteIndented = true
                            })
                        );

                        // Verificar si hubo errores durante la restauración
                        var failedItems = backupMetadata.Items.Where(i => !i.IsRestored).ToList();
                        if (failedItems.Any())
                        {
                            string errorMessage = "Algunos elementos no pudieron ser restaurados:\n\n";
                            foreach (var item in failedItems)
                            {
                                errorMessage += $"- {item.OriginalPath}\n  {item.RestoreErrorMessage}\n";
                            }

                            CustomMsgBox.Show(
                                prompt: errorMessage,
                                title: "Restauración Parcial",
                                buttons: CustomMessageBoxButtons.OK,
                                icon: CustomMessageBoxIcon.Warning,
                                theme: ThemeSettings.DarkTheme
                            );
                        }
                        else
                        {
                            CustomMsgBox.Show(
                                prompt: "La copia de seguridad se ha restaurado correctamente.",
                                title: "Restauración Completada",
                                buttons: CustomMessageBoxButtons.OK,
                                icon: CustomMessageBoxIcon.Information,
                                theme: ThemeSettings.DarkTheme
                            );
                        }
                        }
                        else
                        {
                            throw new FileNotFoundException("No se encontró el archivo manifest.json en el backup.");
                        }
                    }
                    finally
                    {
                        // Limpiar el directorio temporal
                        if (Directory.Exists(tempDir))
                        {
                            Directory.Delete(tempDir, true);
                        }
                    }

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    CustomMsgBox.Show(
                        prompt: $"Error al restaurar la copia de seguridad: {ex.Message}",
                        title: "Error",
                        buttons: CustomMessageBoxButtons.OK,
                        icon: CustomMessageBoxIcon.Error,
                        theme: ThemeSettings.DarkTheme
                    );
                }
                finally
                {
                    btnRestore.Enabled = true;
                    btnCancel.Enabled = true;
                    lstBackups.Enabled = true;
                    Cursor = Cursors.Default;
                }
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
