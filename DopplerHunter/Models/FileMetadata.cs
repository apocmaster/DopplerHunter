using DopplerHunter.Utilities;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Navigation;

namespace DopplerHunter.Models
{
    /// <summary>
    /// Unified model for file metadata, used for grouping duplicates and tracking file properties.
    /// </summary>
    public class FileMetadata: NotificationPropertiesBase
    {

        #region Properties

        /// <summary>
        /// Path of the file, used for grouping duplicates.
        /// </summary>
        public string FullPath { get; set; } = string.Empty;

        /// <summary>
        /// Filename of the file, used for grouping duplicates.
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Size of the file in bytes, used for grouping duplicates.
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Hash SHA256 of the file, used for grouping duplicates.
        /// </summary>
        private string fileHash = string.Empty;
        public string FileHash { get => fileHash; set => fileHash = value; }

        /// <summary>
        /// Last modified date of the file, used for grouping duplicates.
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Indicate whether the hash has been calculated for this file. This is used to avoid recalculating the hash for files that have already been processed.
        /// </summary>
        public bool IsHashCalculated { get; set; }

        /// <summary>
        /// Flag to indicate if the file is a duplicate. This is used for grouping duplicates.
        /// </summary>
        public bool IsFileDuplicated { get; set; }

        /// <summary>
        /// Path of the folder containing the file, used for grouping duplicates.
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

        private FileActionResult actionResult = new();
        public FileActionResult ActionResult 
        { 
            get => actionResult;
            set 
            { 
                if(actionResult != value)
                {
                    actionResult = value;
                    OnPropertyChanged($"{nameof(ActionResult)}");
                }
            } 
        }


        #endregion

        #region Events

        /// <summary>
        /// Event triggered when the selection state of the file changes.
        /// </summary>
        public static event EventHandler? SelectionChanged;

        #endregion

        /// <summary>
        /// Get the formatted file size in KB/MB/GB.
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
        /// Creates a copy of the current FileMetadata instance.
        /// </summary>
        /// <returns>A new FileMetadata instance with the same property values.</returns>
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

        /// <summary>
        /// Returns a string representation of the FileMetadata instance, including the filename and formatted file size.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{FileName} ({FileSizeFormatted})";
        }
    }
}
