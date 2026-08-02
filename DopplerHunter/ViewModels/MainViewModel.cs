using DopplerHunter.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Input;

namespace DopplerHunter.ViewModels
{
    public class MainViewModel: BaseViewModel
    {
        public ObservableCollection<FileSystemItemViewModel> Drives { get; set; }
        public ObservableCollection<FileSystemItemViewModel> FoldersToSearch { get; } = new ObservableCollection<FileSystemItemViewModel>();
        public ICommand ProcessDirectoryCommand { get; set; }
        public ICommand AddFolderToSearchCommand { get; }
        public ICommand RemoveFolderCommand { get; }
        public ICommand ClearSelectionCommand { get; }

        private string _statusMessage = string.Empty;
        public string StatusMessage 
        { 
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); } 
        }

        public MainViewModel()
        {
            Drives = new ObservableCollection<FileSystemItemViewModel>();
            ProcessDirectoryCommand = new RelayCommand(OnProcessDirectory);
            AddFolderToSearchCommand = new RelayCommand(OnAddFolderToSearch);
            RemoveFolderCommand = new RelayCommand(OnRemoveFolder);
            ClearSelectionCommand = new RelayCommand(OnClearSelection);
            LoadDrives();
        }

        private void LoadDrives()
        {
            foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady)) 
                Drives.Add(new FileSystemItemViewModel(drive.Name, drive.RootDirectory.FullName, true));
        }

        private void OnProcessDirectory(object parameter)
        {
            if (parameter is FileSystemItemViewModel item)
            {
                StatusMessage = $"Processing directory: {item.FullPath}";
                // Implement your processing logic here
            }
        }

        private void OnAddFolderToSearch(object parameter)
        {
            if (parameter is FileSystemItemViewModel item)
            {
                if (!FoldersToSearch.Contains(item))
                {
                    FoldersToSearch.Add(item);
                    StatusMessage = $"Added folder to search: {item.FullPath}";
                    Debug.WriteLine($"Added folder to search: {item.FullPath}");
                }
                else
                {
                    StatusMessage = $"Folder is already in the search list: {item.FullPath}";
                    Debug.WriteLine($"Folder is already in the search list: {item.FullPath}");
                }
            }
        }

        private void OnRemoveFolder(object parameter)
        {
            if (parameter is FileSystemItemViewModel item)
            {
                if (FoldersToSearch.Contains(item))
                {
                    FoldersToSearch.Remove(item);
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

        private void OnClearSelection(object parameter)
        {
            FoldersToSearch.Clear();
            StatusMessage = "Cleared all selected folders.";
            Debug.WriteLine("Cleared all selected folders.");
        }
    }
}
