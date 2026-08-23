using DopplerHunter.Utilities;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Navigation;

namespace DopplerHunter.Models
{
    /// <summary>
    /// Modelo unificado para almacenar metadatos de archivos y detección de duplicados.
    /// </summary>
    public class FileMetadata: NotificationPropertiesBase
    {
        
        #region Properties

        /// <summary>
        /// Ruta completa del archivo.
        /// </summary>
        public string FullPath { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del archivo.
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Tamaño del archivo en bytes.
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Hash SHA256 del archivo (calculado después de verificar tamaño).
        /// </summary>
        private string fileHash = string.Empty;
        public string FileHash { get => fileHash; set => fileHash = value; }

        /// <summary>
        /// Fecha de última modificación.
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Indica si el hash ha sido calculado.
        /// </summary>
        public bool IsHashCalculated { get; set; }

        /// <summary>
        /// Clave compuesta para agrupar duplicados (tamaño + hash).
        /// </summary>
        public bool IsFileDuplicated { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string FolderPath { get; set; } = string.Empty;

        public string Extension { get; set; } = string.Empty;

        public int DuplicateIndex { get; set; }

        public string DisplayName => FileNameFormatter.Shorten(FileName);

        public string? ToolTipName =>
            !string.IsNullOrEmpty(FileName) && FileName.Length > 50
                ? FileName
                : null;

        private bool isSelected;

        

        public bool IsSelected 
        { 
            get => isSelected;
            set
            { 
                if(isSelected != value)
                {
                    isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                    SelectionChanged?.Invoke(this, EventArgs.Empty);
                }
            } 
        }

        #endregion

        #region Events

        public static event EventHandler? SelectionChanged;

        #endregion

        /// <summary>
        /// Obtiene el tamaño formateado en KB/MB/GB.
        /// </summary>
        public string FileSizeFormatted
        {
            get
            {
                const long kb = 1024;
                const long mb = kb * 1024;
                const long gb = mb * 1024;

                if (FileSize >= gb)
                    return $"{FileSize / (double)gb:F2} GB";
                if (FileSize >= mb)
                    return $"{FileSize / (double)mb:F2} MB";
                if (FileSize >= kb)
                    return $"{FileSize / (double)kb:F2} KB";
                return $"{FileSize} B";
            }
        }

        /// <summary>
        /// Copia del objeto.
        /// </summary>
        public FileMetadata Clone()
        {
            return new FileMetadata
            {
                FullPath = this.FullPath,
                FileName = this.FileName,
                FileSize = this.FileSize,
                FileHash = this.FileHash,
                LastModified = this.LastModified,
                IsHashCalculated = this.IsHashCalculated
            };
        }

        public override string ToString()
        {
            return $"{FileName} ({FileSizeFormatted})";
        }
    }
}
