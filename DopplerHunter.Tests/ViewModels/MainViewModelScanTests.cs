using DopplerHunter.Services;
using DopplerHunter.ViewModels;
using NSubstitute;
using System.IO;

#nullable disable

namespace DopplerHunter.Tests.ViewModels
{
    public class MainViewModelScanTests
    {
        [Fact]
        public void Should_PreventScanAndShowMessage_WhenNoFoldersSelected()
        {
            RunOnSta(async () =>
            {
                // Arrange
                var mockDriveService = Substitute.For<IDriveService>();
                mockDriveService.GetDrives().Returns(new[] { new DriveInfo("C") });
                var mockFileService = Substitute.For<IFileService>();
                var mockDirectoryService = Substitute.For<IDirectoryService>();

                var vm = new MainViewModel(mockDriveService, mockFileService, mockDirectoryService);

                // Act
                vm.ScanForDuplicatesCommand.Execute(null);
                await Task.Delay(50);

                // Assert
                Assert.False(vm.IsScanning);
                Assert.Equal("Please select at least one folder to search.", vm.StatusMessage);
                Assert.Equal("Search _Duplicates", vm.SearchButtonText);
            });
        }

        [Fact]
        public void Should_ExecuteScanAndResetIsScanning_WhenFoldersAreSelected()
        {
            RunOnSta(async () =>
            {
                // Arrange
                var mockDriveService = Substitute.For<IDriveService>();
                mockDriveService.GetDrives().Returns(new[] { new DriveInfo("C") });
                var mockFileService = Substitute.For<IFileService>();
                var mockDirectoryService = Substitute.For<IDirectoryService>();

                var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempDir);

                mockDirectoryService.ScanDirectoriesAndSubdirectories(Arg.Any<List<FileSystemItemViewModel>>())
                    .Returns(Task.FromResult(new List<DirectoryInfo> { new DirectoryInfo(tempDir) }));
                mockDirectoryService.GetFilesInDirectories(Arg.Any<List<DirectoryInfo>>())
                    .Returns(Task.FromResult(new List<FileInfo>()));

                var vm = new MainViewModel(mockDriveService, mockFileService, mockDirectoryService);
                vm.SelectedSearchFolders.Add(new FileSystemItemViewModel("Temp", tempDir, false));

                bool wasScanning = false;
                vm.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(MainViewModel.IsScanning) && vm.IsScanning)
                    {
                        wasScanning = true;
                    }
                };

                // Act
                vm.ScanForDuplicatesCommand.Execute(null);
                await Task.Delay(150);

                // Assert
                Assert.True(wasScanning);
                Assert.False(vm.IsScanning);
                Assert.Equal("Search _Duplicates", vm.SearchButtonText);
                Assert.Contains("Scan completed", vm.StatusMessage);

                // Cleanup
                Directory.Delete(tempDir, true);
            });
        }

        [Fact]
        public void Should_DisableScanCommand_WhenIsScanningIsTrue()
        {
            RunOnSta(() =>
            {
                // Arrange
                var mockDriveService = Substitute.For<IDriveService>();
                mockDriveService.GetDrives().Returns(new[] { new DriveInfo("C") });
                var mockFileService = Substitute.For<IFileService>();
                var mockDirectoryService = Substitute.For<IDirectoryService>();

                var vm = new MainViewModel(mockDriveService, mockFileService, mockDirectoryService);

                // Assert initially can execute
                Assert.True(vm.ScanForDuplicatesCommand.CanExecute(null));
                Assert.Equal("Search _Duplicates", vm.SearchButtonText);
            });
        }

        private static void RunOnSta(Func<Task> asyncAction)
        {
            Exception exception = null;
            var thread = new Thread(() =>
            {
                try
                {
                    asyncAction().GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (exception != null)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception).Throw();
            }
        }

        private static void RunOnSta(Action action)
        {
            RunOnSta(() =>
            {
                action();
                return Task.CompletedTask;
            });
        }
    }
}
