using DopplerHunter.Services;
using DopplerHunter.ViewModels;
using NSubstitute;



namespace DopplerHunter.Tests.ViewModels
{
    public class MainViewModelTests
    {
        [Fact]
        public void Should_LoadFakeDrives_WhenInitialized()
        {
            // Arrange - Crear mock del servicio
            var mockDriveService = Substitute.For<IDriveService>();
            mockDriveService.GetDrives().Returns(new[] { new DriveInfo("C") });
            var mockFileService = Substitute.For<IFileService>();
            var mockDirectoryService = Substitute.For<IDirectoryService>();

            // Act
            var viewModel = new MainViewModel(mockDriveService, mockFileService, mockDirectoryService);

            // Assert
            Assert.NotNull(viewModel.Drives);
            Assert.Single(viewModel.Drives);
            Assert.Equal("C:\\", viewModel.Drives.First().Name);
        }
    }
}

