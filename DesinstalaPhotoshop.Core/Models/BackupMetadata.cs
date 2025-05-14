using System;
using System.Collections.Generic;

namespace DesinstalaPhotoshop.Core.Models
{
    /// <summary>
    /// Representa los metadatos de una copia de seguridad
    /// </summary>
    public class BackupMetadata
    {
        /// <summary>
        /// Identificador único de la copia de seguridad
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Fecha y hora de creación de la copia de seguridad
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Fecha y hora de la copia de seguridad (compatibilidad)
        /// </summary>
        public DateTime BackupTime { get; set; }

        /// <summary>
        /// Tipo de operación (Cleanup, Uninstall, etc.)
        /// </summary>
        public string OperationType { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de copia de seguridad (Cleanup, Uninstall, etc.)
        /// </summary>
        public BackupType BackupType { get; set; }

        /// <summary>
        /// Nombre de la instalación de Photoshop
        /// </summary>
        public string InstallationName { get; set; } = string.Empty;

        /// <summary>
        /// Versión de Photoshop
        /// </summary>
        public string PhotoshopVersion { get; set; } = string.Empty;

        /// <summary>
        /// Ubicación de la instalación de Photoshop
        /// </summary>
        public string InstallationLocation { get; set; } = string.Empty;

        /// <summary>
        /// Ruta de la copia de seguridad
        /// </summary>
        public string BackupPath { get; set; } = string.Empty;

        /// <summary>
        /// Notas adicionales sobre la copia de seguridad
        /// </summary>
        public string Notes { get; set; } = string.Empty;

        /// <summary>
        /// Número total de elementos respaldados
        /// </summary>
        public int ItemCount { get; set; }

        /// <summary>
        /// Tamaño total de la copia de seguridad en bytes
        /// </summary>
        public long TotalSize { get; set; }

        /// <summary>
        /// Lista de elementos respaldados
        /// </summary>
        public List<BackupItem> Items { get; set; } = new List<BackupItem>();

        /// <summary>
        /// Lista de archivos respaldados
        /// </summary>
        public List<string> BackedUpFiles { get; set; } = new List<string>();

        /// <summary>
        /// Lista de claves de registro respaldadas
        /// </summary>
        public List<string> BackedUpRegistryKeys { get; set; } = new List<string>();
    }

    /// <summary>
    /// Representa un elemento respaldado (archivo, directorio o clave de registro)
    /// </summary>
    public class BackupItem
    {
        /// <summary>
        /// Ruta original del elemento
        /// </summary>
        public string OriginalPath { get; set; } = string.Empty;

        /// <summary>
        /// Ruta donde se ha guardado la copia de seguridad
        /// </summary>
        public string BackupPath { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de elemento (Archivo, Directorio, RegistryKey)
        /// </summary>
        public BackupItemType ItemType { get; set; }

        /// <summary>
        /// Tamaño del elemento en bytes
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Fecha y hora de la copia de seguridad
        /// </summary>
        public DateTime BackupTime { get; set; }

        /// <summary>
        /// Indica si el elemento ha sido restaurado
        /// </summary>
        public bool IsRestored { get; set; }

        /// <summary>
        /// Fecha y hora de la restauración (si aplica)
        /// </summary>
        public DateTime? RestoreTime { get; set; }

        /// <summary>
        /// Mensaje de error durante la restauración (si aplica)
        /// </summary>
        public string? RestoreErrorMessage { get; set; }
    }

    /// <summary>
    /// Tipo de copia de seguridad
    /// </summary>
    public enum BackupType
    {
        /// <summary>
        /// Copia de seguridad antes de limpieza
        /// </summary>
        Cleanup,

        /// <summary>
        /// Copia de seguridad antes de desinstalación
        /// </summary>
        Uninstall
    }

    /// <summary>
    /// Tipo de elemento respaldado
    /// </summary>
    public enum BackupItemType
    {
        /// <summary>
        /// Archivo
        /// </summary>
        File,

        /// <summary>
        /// Directorio
        /// </summary>
        Directory,

        /// <summary>
        /// Clave de registro
        /// </summary>
        RegistryKey
    }
}