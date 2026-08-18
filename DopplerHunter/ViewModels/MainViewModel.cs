using DopplerHunter.Commands;
using DopplerHunter.Models;
using DopplerHunter.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace DopplerHunter.ViewModels
{
    public class MainViewModel: BaseViewModel
    {
        #region Properties
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

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set 
            { 
                _statusMessage = value;
                Debug.WriteLine(_statusMessage); // TODO : Remove this line after debugging
                OnPropertyChanged(); 
            }
        }

        private HashSet<string> _processedDirectories = new(StringComparer.OrdinalIgnoreCase);

        #endregion

        #region Commands
        public ICommand ToggleExpandCommand { get; set; }
        public ICommand SelectFolderForSearchCommand { get; }
        public ICommand ExcludeFolderFromSearchCommand { get; }
        public ICommand ClearFoldersFromSearchCommand { get; }
        public ICommand ScanForDuplicatesCommand { get; }

        public ICommand OpenFileCommand { get; }

        #endregion

        #region Services
        private readonly IDriveService _driveService;
        private readonly IFileService _fileService;

        #endregion

        #region Constructor
        public MainViewModel() : this(new DriveService(), new FileService())
        {
        }

        public MainViewModel(IDriveService driveService, IFileService fileService)
        {
            _driveService = driveService;
            _fileService = fileService;

            ToggleExpandCommand = new RelayCommand(async (p) => await OnToggleExpandCommand(p));
            SelectFolderForSearchCommand = new RelayCommand(OnSelectFolderForSearchCommand);
            ExcludeFolderFromSearchCommand = new RelayCommand(OnExcludeFolderFromSearchCommand);
            ClearFoldersFromSearchCommand = new RelayCommand(OnClearFoldersFromSearchCommand);
            ScanForDuplicatesCommand = new RelayCommand(async (p) => await OnScanForDuplicatesCommand(p));
            OpenFileCommand = new RelayCommand(async (p) => await OnOpenFileCommand(p.ToString()!));

            FilesFoundView = CollectionViewSource.GetDefaultView(FilesFound);
            
            FilesFoundView.SortDescriptions.Add(new SortDescription(nameof(FileMetadata.FileHash), ListSortDirection.Ascending));
            FilesFoundView.SortDescriptions.Add(new SortDescription(nameof(FileMetadata.FolderPath), ListSortDirection.Ascending));

            FilesFound.CollectionChanged += (s, e) => ApplyGrouping();

            ApplyGrouping();
            LoadDrives();
        }

        #endregion 

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
                }
            }
        }

        private void OnExcludeFolderFromSearchCommand(object parameter)
        {
            if (parameter is FileSystemItemViewModel item)
            {
                StatusMessage = (SelectedSearchFolders.Remove(item)) ?
                    $"Removed folder from search: {item.FullPath}" :
                    $"Folder not found in the search list: {item.FullPath}";                
            }
        }

        private void OnClearFoldersFromSearchCommand(object parameter)
        {
            SelectedSearchFolders.Clear();
            StatusMessage = "Cleared all selected folders.";
        }

        //=================================
        // Files Actions
        //=================================
        private async Task OnOpenFileCommand(string parameter)
        { 
            if(_fileService.IsFileNotExists(parameter))
            {
                StatusMessage = $"File does not exist: {parameter}";
                return;
            }
                        
            StatusMessage = await _fileService.OpenFileAsync(parameter);
        }

        /// <summary>
        /// Searches for duplicate files in the selected folders. It clears the previous search results, resets the counters, and initiates a search in each folder, optionally including subdirectories.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private async Task OnScanForDuplicatesCommand(object parameter)
        {
            StatusMessage = "Searching files in folders.";
            if(IsThereAnyFolderSelected())
            {
                CleanFilesFoundCollection();
                ResetCounters();
                ResetProcessedDirectories();

                // Search for files in each selected folders
                await ScanSelectedFoldersForFiles();
                

                var possibleDuplicates = FilesFound
                    .GroupBy(f => f.FileSize)
                    .Where(g => g.Count() > 1)
                    .SelectMany(g => g);

                int counter = 0;
                foreach (var file in possibleDuplicates)
                {
                    var hash = await _fileService.ComputeXXHash(file.FullPath);
                    file.FileHash = hash;
                    file.IsHashCalculated = true;                    
                    
                    counter++;
                    if (counter % 10 == 0)
                    {
                        await Task.Yield(); // Yield control to keep UI responsive
                    }                    
                }

                var duplicates = FilesFound
                    .Where(f => f.IsHashCalculated && !string.IsNullOrEmpty(f.FileHash))
                    .GroupBy(f => f.FileHash)
                    .Where(g => g.Count() > 1)
                    .SelectMany(g => g);

                foreach (var file in duplicates)
                {
                    file.IsFileDuplicated = true;
                    StatusMessage = $"Duplicate found: {file.FullPath} (Hash: {file.FileHash})";
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
        /// Checks if there are any folders selected for searching. It returns true if the SelectedSearchFolders collection has one or more items, otherwise false.
        /// </summary>
        /// <returns></returns>
        private bool IsThereAnyFolderSelected()
        {
            return SelectedSearchFolders.Count > 0;
        }

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

        private async Task ScanSelectedFoldersForFiles()
        {
            foreach (var folder in SelectedSearchFolders)
            {
                await SearchFilesInDirectory(folder.FullPath, folder.IncludeSubdirectories);
                await UpdateTotalFilesFound();
            }
        }


        /// <summary>
        /// Searches for files in the specified directory. If includeSubdirectories is true, it recursively searches through all subdirectories. It registers the contents of the directory by adding its files to the FilesFound collection and updating the total counts of files and folders found. 
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="includeSubdirectories"></param>
        /// <returns></returns>
        private async Task SearchFilesInDirectory(string folder, bool includeSubdirectories)
        {
            if (includeSubdirectories)
            {
                await ProcessSubdirectories(folder);
            }
            
            if (CanProcessDirectory(folder)) return;

            await IncreaseTotalFoldersFound();
            await RegisterDirectoryContents(folder);            
        }

        /// <summary>
        /// Processes the subdirectories of the specified folder by recursively searching for files in each subdirectory. It calls the SearchFilesInDirectory method for each subdirectory found.
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        private async Task ProcessSubdirectories(string folder)
        {
            foreach (var subDir in Directory.GetDirectories(folder))
            {
                await SearchFilesInDirectory(subDir, true);
            }
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
        }

        /// <summary>
        /// Increases the total count of folders found by one.
        /// </summary>
        private async Task IncreaseTotalFoldersFound()
        {
            TotalFoldersFound++;
            OnPropertyChanged(nameof(TotalFoldersFound));
        }

        /// <summary>
        /// Updates the total count of files found based on the current count of the FilesFound collection.
        /// </summary>
        private async Task UpdateTotalFilesFound()
        {
            TotalFilesFound = FilesFound.Count;
            OnPropertyChanged(nameof(TotalFilesFound));
        }

        /// <summary>
        /// Resets the total counts of files and folders found to zero.
        /// </summary>
        private void ResetCounters()
        {
            TotalFilesFound = 0;
            TotalFoldersFound = 0;
            TotalDuplicatesFound = 0;
        }

        private bool CanProcessDirectory(string directoryPath)
        {
            return !_processedDirectories.Add(directoryPath);
        }
        private void ResetProcessedDirectories()
        {
            _processedDirectories.Clear();
        }
        /// <summary>
        /// Clears the FilesFound collection and notifies that the property has changed.
        /// </summary>
        private void CleanFilesFoundCollection()
        {
            FilesFound.Clear();
            OnPropertyChanged(nameof(FilesFound));
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

        

        private void ApplyGrouping()
        {
            if (FilesFoundView == null) return;

            FilesFoundView.GroupDescriptions.Clear();

            if (FilesFound.Count > 0)
            {
                FilesFoundView.GroupDescriptions.Add(
                    new PropertyGroupDescription(nameof(FileMetadata.FileHash)));
            }
        }
    }
}
