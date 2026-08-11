using DopplerHunter.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace DopplerHunter.Tests.ViewModels
{
    public class FileSystemItemViewModelTests
    {
        [Theory]
        [InlineData("TestFolder", @"C:\TestFolder", false)]
        [InlineData("AnotherFolder", @"C:\AnotherFolder", true)]
        public void Should_ReturnCorrectValues_WhenInitialized(string name, string fullPath, bool isDrive)
        {
            // Arrange

            // Act
            var viewModel = new FileSystemItemViewModel(name, fullPath, isDrive);

            // Assert
            Assert.Equal(name, viewModel.Name);
            Assert.Equal(fullPath, viewModel.FullPath);
            Assert.Equal(isDrive, viewModel.IsDrive);
        }
    }
}
