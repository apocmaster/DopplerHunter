using DopplerHunter.Adapters;
using DopplerHunter.Commands;
using DopplerHunter.Events;
using DopplerHunter.Extensions;
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

        // TODO : Move this to DirectoryService
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
        private readonly IDirectoryService _directoryService;

        #endregion

        #region Constructor
        public MainViewModel() : this(new DriveService(), new FileService(), new DirectoryService())
        {
        }

        public MainViewModel(IDriveService driveService, IFileService fileService, IDirectoryService directoryService)
        {
            _driveService = driveService;
            _fileService = fileService;
            _directoryService = directoryService;

            _fileService.HashesCalculated += OnHashesCalulated;

            _directoryService.DirectoryAnalized += OnDirectoryAnalized;
            _directoryService.FilesExtractionCompleted += OnFilesExtractionCompleted;

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

        #region Events

        private void OnDirectoryAnalized(object? sender, DirectoryAnalizedEventArgs e)
        {
            TotalFoldersFound = e.DirectoriesAnalized;
            OnPropertyChanged(nameof(TotalFoldersFound));
        }

        private void OnFilesExtractionCompleted(object? sender, FilesExtractionCompletedEventArgs e)
        {
            TotalFilesFound += e.FilesExtracted;
            OnPropertyChanged(nameof(TotalFilesFound));
        }

        private void OnHashesCalulated(object? sender, HashesCalculatedEventArgs e)
        {
            TotalDuplicatesFound = e.HasesCalculated;
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
            if (IsThereAnyFolderSelected())
            {
                CleanFilesFoundCollection();
                ResetCounters();
                ResetProcessedDirectories();

                var directories = await _directoryService.ScanDirectoriesAndSubdirectories(
                    SelectedSearchFolders.ToList());

                var files = await _directoryService.GetFilesInDirectories(directories);

                var filesMetadataList = FileInfoToFileMetadataAdapter.Convert(files);
                FilesFound.AddRange(filesMetadataList);


                await _fileService.CalculatePossibleDuplicates(FilesFound);
                await _fileService.MarkFilesDuplicates(FilesFound);
                await _fileService.GroupFilesDuplicated(FilesFound);
                
                OnPropertyChanged(nameof(FilesFound)); //notificar los cambios
                FilesFoundView.Filter = f => ((FileMetadata)f).IsFileDuplicated; // Filter to show only duplicates
                FilesFoundView.Refresh();

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

        /// <summary>
        /// Resets the total counts of files and folders found to zero.
        /// </summary>
        private void ResetCounters()
        {
            TotalFilesFound = 0;
            TotalFoldersFound = 0;
            TotalDuplicatesFound = 0;
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
