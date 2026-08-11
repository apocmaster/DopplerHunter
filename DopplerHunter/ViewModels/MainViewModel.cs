using DopplerHunter.Commands;
using DopplerHunter.Models;
using DopplerHunter.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Data;
using System.Windows.Input;

namespace DopplerHunter.ViewModels
{
    public class MainViewModel: BaseViewModel
    {
        public ObservableCollection<FileSystemItemViewModel> Drives { get; } = [];
        public ObservableCollection<FileSystemItemViewModel> SelectedSearchFolders { get; } = [];
        public ObservableCollection<FileMetadata> FilesFound { get; } = [];

        public ICollectionView FilesFoundView { get; }

        private long totalFilesFound;

        public long TotalFilesFound
        {
            get { return totalFilesFound; }
            private set {
                if (totalFilesFound != value)
                {
                    totalFilesFound = value;
                    OnPropertyChanged(nameof(TotalFilesFound));
                }
            }
        }

        private long totalFolderFound;

        public long TotalFoldersFound
        {
            get { return totalFolderFound; }
            private set { 
                if(totalFolderFound != value)
                totalFolderFound = value;
                OnPropertyChanged(nameof(TotalFoldersFound));
            }
        }

        private long totalDuplicatesFound;

        public long TotalDuplicatesFound
        {
            get { return totalDuplicatesFound; }
            private set { 
                if(totalDuplicatesFound != value)
                totalDuplicatesFound = value;
                OnPropertyChanged(nameof(TotalDuplicatesFound));
            }
        }

        public ICommand ToggleExpandCommand { get; set; }
        public ICommand SelectFolderForSearchCommand { get; }
        public ICommand ExcludeFolderFromSearchCommand { get; }
        public ICommand ClearFoldersFromSearchCommand { get; }
        public ICommand ScanForDuplicatesCommand { get; }

        public ICommand OpenFileCommand { get; }

        private string _statusMessage = string.Empty;
        private readonly IDriveService _driveService;

        public string StatusMessage 
        { 
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); } 
        }

        public MainViewModel() : this(new DriveService())
        {
        }

        public MainViewModel(IDriveService driveService)
        {
            _driveService = driveService;

            ToggleExpandCommand = new RelayCommand(async (p) => await OnToggleExpandCommand(p));
            SelectFolderForSearchCommand = new RelayCommand(OnSelectFolderForSearchCommand);
            ExcludeFolderFromSearchCommand = new RelayCommand(OnExcludeFolderFromSearchCommand);
            ClearFoldersFromSearchCommand = new RelayCommand(OnClearFoldersFromSearchCommand);
            ScanForDuplicatesCommand = new RelayCommand(async (p) => await OnScanForDuplicatesCommand(p));
            OpenFileCommand = new RelayCommand(async (p) => await OnOpenFileCommand(p.ToString()!));

            FilesFoundView = CollectionViewSource.GetDefaultView(FilesFound);
            FilesFoundView.SortDescriptions.Add(new SortDescription(nameof(FileMetadata.FileHash), ListSortDirection.Ascending));
            FilesFoundView.SortDescriptions.Add(new SortDescription(nameof(FileMetadata.FolderPath), ListSortDirection.Ascending));
            LoadDrives();
        }


        #region Controls

        private async Task OnToggleExpandCommand(object parameter)
        {
            if (parameter is FileSystemItemViewModel item)
            {
                StatusMessage = $"Processing directory: {item.FullPath}";
                // Implement your processing logic here
            }
        }

        #endregion


        #region Commands and Actions

        // ==================================
        // Directories Actions
        // ==================================
        private void OnSelectFolderForSearchCommand(object parameter)
        {
            if (parameter is FileSystemItemViewModel item)
            {
                if (!SelectedSearchFolders.Contains(item))
                {
                    SelectedSearchFolders.Add(item);
                    StatusMessage = $"Added folder to search: {item.FullPath}";
                    Debug.WriteLine($"Added folder to search: {item.FullPath}");
                }
            }
        }

        private void OnExcludeFolderFromSearchCommand(object parameter)
        {
            if (parameter is FileSystemItemViewModel item)
            {
                if (SelectedSearchFolders.Contains(item))
                {
                    SelectedSearchFolders.Remove(item);
                    StatusMessage = $"Removed folder from search: {item.FullPath}";
                    Debug.WriteLine($"Removed folder from search: {item.FullPath}");
                }
                else
                {
                    StatusMessage = $"Folder not found in the search list: {item.FullPath}";
                    Debug.WriteLine($"Folder not found in the search list: {item.FullPath}");
                }
            }
        }

        private void OnClearFoldersFromSearchCommand(object parameter)
        {
            SelectedSearchFolders.Clear();
            StatusMessage = "Cleared all selected folders.";
            Debug.WriteLine("Cleared all selected folders.");
        }

        //=================================
        // Files Actions
        //=================================
        private async Task OnOpenFileCommand(string parameter)
        { 
            if(File.Exists(parameter))
            {
                try
                {
                    Process.Start(new ProcessStartInfo(parameter) { UseShellExecute = true });
                    StatusMessage = $"Opened file: {parameter}";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error opening file: {ex.Message}";
                }
            }
            else
            {
                StatusMessage = $"File does not exist: {parameter}";
            }
            Debug.WriteLine("Opening file.");
        }

        /// <summary>
        /// Searches for duplicate files in the selected folders. It clears the previous search results, resets the counters, and initiates a search in each folder, optionally including subdirectories.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private async Task OnScanForDuplicatesCommand(object parameter)
        {
            // Implement duplicate search logic here
            Debug.WriteLine("Searching files in folders.");
            if(SelectedSearchFolders.Count > 0)
            {
                FilesFound.Clear();
                ResetCounters();
                // Search for files in each selected folders
                foreach (var folder in SelectedSearchFolders)
                {
                    await SearchFilesInDirectory(folder.FullPath, folder.IncludeSubdirectories);
                    Debug.WriteLine($"Searching in folder: {folder.FullPath}");
                    // Call your file search service here
                }
                
                var possibleDuplicates = FilesFound
                    .GroupBy(f => f.FileSize)
                    .Where(g => g.Count() > 1)
                    .SelectMany(g => g);

                foreach (var file in possibleDuplicates)
                {
                    var hash = ComputeMD5(file.FullPath);
                    file.FileHash = hash;
                    file.IsHashCalculated = true;                    
                        
                        Debug.WriteLine($"Possible duplicate found: {file.FullPath} (Size: {file.FileSize})");
                }

                var duplicates = FilesFound
                    .Where(f => f.IsHashCalculated && !string.IsNullOrEmpty(f.FileHash))
                    .GroupBy(f => f.FileHash)
                    .Where(g => g.Count() > 1)
                    .SelectMany(g => g);

                foreach (var file in duplicates)
                {
                    file.IsFileDuplicated = true;
                    Debug.WriteLine($"Duplicate found: {file.FullPath} (Hash: {file.FileHash})");
                }

                var grouped = FilesFound
                .Where(f => f.IsFileDuplicated)
                .GroupBy(f => f.FileHash)
                .Select(g => g.OrderBy(f => f.FolderPath))
                ;

                foreach (var group in grouped)
                {
                    int index = 1;
                    foreach (var file in group)
                    {
                        file.DuplicateIndex = index++;
                    }
                }


                OnPropertyChanged(nameof(FilesFound)); //notificar los cambios
                FilesFoundView.Filter = f => ((FileMetadata)f).IsFileDuplicated; // Filter to show only duplicates
                FilesFoundView.Refresh();


                TotalDuplicatesFound = FilesFound.Count(f => f.IsFileDuplicated == true);
                OnPropertyChanged(nameof(TotalDuplicatesFound));
            }
        }

        #endregion

        /// <summary>
        /// Loads the available drives on the system and adds them to the Drives collection. It filters out drives that are not ready (e.g., empty CD/DVD drives) and creates a FileSystemItemViewModel for each ready drive.
        /// </summary>
        private void LoadDrives()
        {
            var drives = _driveService.GetDrives().Where(d => d.IsReady);
            foreach (var drive in drives)
            {
                Drives.Add(new FileSystemItemViewModel(drive.Name, drive.RootDirectory.FullName, true));
            }
        }

        /// <summary>
        /// Searches for files in the specified directory and optionally in its subdirectories. It registers the contents of each directory found.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="includeSubdirectories"></param>
        /// <returns></returns>
        private async Task SearchFilesInDirectory(string folder, bool includeSubdirectories)
        {
            if (includeSubdirectories)
            {
                var subDirectories = Directory.GetDirectories(folder);

                foreach (var subDir in subDirectories)
                {
                    await SearchFilesInDirectory(subDir, includeSubdirectories);
                }
            }

            await RegisterDirectoryContents(folder);            
        }

        /// <summary>
        /// Registers the contents of a directory by adding its files to the FilesFound collection and updating the total counts of files and folders found.
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        private async Task RegisterDirectoryContents(string folder)
        {
            var directoryFiles = new DirectoryInfo(folder);
            await AddFilesToFoundCollection(directoryFiles.GetFiles());


            IncreaseTotalFoldersFound();
            UpdateTotalFilesFound();
        }

        /// <summary>
        /// Increases the total count of folders found by one.
        /// </summary>
        private void IncreaseTotalFoldersFound()
        {
            TotalFoldersFound++;
        }

        /// <summary>
        /// Updates the total count of files found based on the current count of the FilesFound collection.
        /// </summary>
        private void UpdateTotalFilesFound()
        {
            TotalFilesFound = FilesFound.Count;
        }

        /// <summary>
        /// Resets the total counts of files and folders found to zero.
        /// </summary>
        private void ResetCounters()
        {
            TotalFilesFound = 0;
            TotalFoldersFound = 0;
        }

        /// <summary>
        /// Adds the specified files to the FilesFound collection by creating FileMetadata objects for each file and populating their properties.
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        private async Task AddFilesToFoundCollection(FileInfo[] files)
        {
            if(files.Length == 0)
                return;

            foreach (var file in files)
            {
                FilesFound.Add(new FileMetadata
                {
                    FullPath = file.FullName,
                    FileName = Path.GetFileNameWithoutExtension(file.Name),                    
                    FileSize = file.Length,
                    LastModified = file.LastWriteTime,
                    IsHashCalculated = false,
                    FolderPath = Path.GetFileName(file.DirectoryName) ?? string.Empty,
                    Extension = Path.GetExtension(file.FullName) ?? string.Empty,
                   
                });
            }
        }

        public static string ComputeMD5(string filePath)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filePath);
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        
    }
}
