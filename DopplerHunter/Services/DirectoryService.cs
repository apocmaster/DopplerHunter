using DopplerHunter.Events;
using DopplerHunter.ViewModels;
using System.IO;


namespace DopplerHunter.Services
{
    public class DirectoryService : IDirectoryService
    {
        private readonly HashSet<string> _processedDirectories;
        
        public event EventHandler<DirectoryAnalizedEventArgs>? DirectoryAnalized;
        public event EventHandler<FilesExtractionCompletedEventArgs>? FilesExtractionCompleted;


        public DirectoryService()
        {
            _processedDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        
        protected virtual void OnFilesExtractionCompleted(List<FileInfo> files) => FilesExtractionCompleted?.Invoke(this, new FilesExtractionCompletedEventArgs(files));
        
        protected virtual void OnDirectoryAnalized(int directory) => DirectoryAnalized?.Invoke(this, new DirectoryAnalizedEventArgs(directory));

        public async Task<List<DirectoryInfo>> ScanDirectoriesAndSubdirectories(List<FileSystemItemViewModel> directoriesSelected)
        {
            var result = new List<DirectoryInfo>();
            if (directoriesSelected == null) return result;

            ResetProcessedDirectories();

            foreach (var directorySelected in directoriesSelected)
            {
                var directoryInfo = new DirectoryInfo(directorySelected.FullPath);
                if(directoryInfo.Exists)
                {
                    await ScanSubdirectories(directoryInfo, directorySelected.IncludeSubdirectories, result);
                }

                OnDirectoryAnalized(result.Count);
            }

            return result;
        }

        public async Task ScanSubdirectories(DirectoryInfo directory, bool includeSubdirectories, List<DirectoryInfo> result)
        {

            if(CanProcessDirectory(directory.FullName)) return; 
            
            result.Add(directory);
            if (includeSubdirectories)
            {
                DirectoryInfo[] subdirectories;
                try
                {
                    subdirectories = directory.GetDirectories();
                }
                catch (UnauthorizedAccessException)
                {
                    // Handle the case where access to a directory is denied
                    return;
                }
                catch (IOException)
                {
                    // Handle other IO exceptions if necessary
                    return;
                }
                foreach (var subdirectory in subdirectories)
                {
                    await ScanSubdirectories(subdirectory, includeSubdirectories, result);
                }
            }
        }

        public async Task<List<FileInfo>> GetFilesInDirectories(List<DirectoryInfo> directories)
        {
            List<FileInfo> files = [];
            foreach (DirectoryInfo directory in directories)
            {
                files.AddRange(await GetFilesInDirectory(directory));
            }

            return files;
        }

        public async Task<List<FileInfo>> GetFilesInDirectory(DirectoryInfo directory)
        {
            List<FileInfo> files = [];

            files = directory.GetFiles().ToList();
            OnFilesExtractionCompleted(files);

            return files;
        }

        private bool CanProcessDirectory(string directoryPath)
        {
            return !_processedDirectories.Add(directoryPath);
        }

        private void ResetProcessedDirectories()
        {
            _processedDirectories.Clear();
        }
    }
}
