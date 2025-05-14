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
        /// Fecha y hora de la copia de seguridad
        /// </summary>
        public DateTime BackupTime { get; set; }

        /// <summary>
        /// Tipo de copia de seguridad (Cleanup, Uninstall, etc.)
        /// </summary>
        public BackupType BackupType { get; set; }

        /// <summary>
        /// Ruta de la copia de seguridad
        /// </summary>
        public string BackupPath { get; set; } = string.Empty;

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