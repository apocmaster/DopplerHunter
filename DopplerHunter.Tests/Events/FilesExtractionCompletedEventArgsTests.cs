using DopplerHunter.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace DopplerHunter.Tests.Events
{
    public class FilesExtractionCompletedEventArgsTests
    {
        [Fact]
        public void Construct_WithFiles_SetFilesExtractedCorrectly()
        {
            // Arrange
            var files = new List<FileInfo>
            { 
                new FileInfo("test01.text"),
                new FileInfo("test01.text")
            };

            // Act
            var result = new FilesExtractionCompletedEventArgs(files);

            // Assert
            Assert.Equal(2, result.FilesExtracted);
        }

        [Fact]
        public void Constrruct_WithEmptyList_SetFilesExtractedToZero()
        {
            // Arrange
            var files = new List<FileInfo>();

            // Act
            var result = new FilesExtractionCompletedEventArgs(files);
            
            // Assert
            Assert.Equal(0, result.FilesExtracted);
        }
    }
}
