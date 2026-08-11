using System;
using System.Collections.Generic;
using System.Text;


namespace DopplerHunter.Tests.Converters
{
    public class BoolToVisibilityConverterTests
    {

        [Fact]
        public void BoolToVisibilityConverter_Convert_ShouldReturnVisible_WhenTrue()
        {
            // Arrange
            var converter = new DopplerHunter.Converters.BoolToVisibilityConverter();
            // Act
            var result = converter.Convert(true, null, null, null);
            // Assert
            Assert.Equal(System.Windows.Visibility.Visible, result);
        }

        [Fact]
        public void BoolToVisibilityConverter_Convert_ShouldReturnCollapsed_WhenFalse()
        {
            // Arrange
            var converter = new DopplerHunter.Converters.BoolToVisibilityConverter();
            // Act
            var result = converter.Convert(false, null, null, null);
            // Assert
            Assert.Equal(System.Windows.Visibility.Collapsed, result);
        }
    }
}
