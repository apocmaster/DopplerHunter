using DopplerHunter.Events;

namespace DopplerHunter.Tests.Events
{
    public class HashesCalculatedEventArgsTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void Constructor_WithValues_ReturnSameValues(int value)
        {
            // Arrange
            var expected = value;
            // Act
            var result = new HashesCalculatedEventArgs(value);
            // Assert
            Assert.Equal(expected, result.HashesCalculated);
        }
    }
}
