using DopplerHunter.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace DopplerHunter.Tests.Events
{
    public class DirectoryAnalizedEventArgsTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void Constructor_WithValue_ReturnSameValue(int directoriesAnalized)
        {
            // Arrange
            var expected = directoriesAnalized;
            // Act
            var result = new DirectoryAnalizedEventArgs(directoriesAnalized);
            // Assert
            Assert.Equal(expected, result.DirectoriesAnalized);
        }
    }
}
