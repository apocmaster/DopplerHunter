using DopplerHunter.Services;
using DopplerHunter.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace DopplerHunter.Tests.Services
{
    public class DirectoryServicesTests
    {
        [Fact]
        public async Task ScanDirectories_ShouldRaiseEvent()
        {
            // Arrange
            IDirectoryService service = new DirectoryService();
            bool eventRaised = false;
            service.DirectoryAnalized += (s, e) => eventRaised = true;

            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            var selection = new List<FileSystemItemViewModel>
            {
                new FileSystemItemViewModel ("", fullPath:tempDir, false )
            };

            // Act
            var result = await service.ScanDirectoriesAndSubdirectories(selection);

            // Assert
            Assert.True(eventRaised);
            Assert.Single(result);

            // Cleanup
            Directory.Delete(tempDir, true);
        }

        [Fact]
        public async Task ScanDirectories_ShouldIncludeSubdirectories_WhenFlagIsTrue()
        {
            // Arrange
            var service = new DirectoryService();
            var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var sub = Path.Combine(root, "Sub");
            Directory.CreateDirectory(root);
            Directory.CreateDirectory(sub);

            var selection = new List<FileSystemItemViewModel>
            {
                new FileSystemItemViewModel ("", root, true )
            };

            // Act
            var result = await service.ScanDirectoriesAndSubdirectories(selection);

            // Assert
            Assert.Contains(result, d => d.FullName == root);
            Assert.Contains(result, d => d.FullName == sub);

            // Cleanup
            Directory.Delete(root, true);
        }

        [Fact]
        public async Task ScanDirectories_ShouldNotProcessDuplicateDirectories()
        {
            // Arrange
            var service = new DirectoryService();
            var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(root);

            var selection = new List<FileSystemItemViewModel>
            {
                new FileSystemItemViewModel ("", root, false ),
                new FileSystemItemViewModel ("", root, false )
            };

            // Act
            var result = await service.ScanDirectoriesAndSubdirectories(selection);

            // Assert
            Assert.Single(result); // solo una vez
            Directory.Delete(root, true);
        }
    }
}
