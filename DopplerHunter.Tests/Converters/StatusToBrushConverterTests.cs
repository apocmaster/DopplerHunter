using DopplerHunter.Converters;
using System.Globalization;
using System.Windows.Media;
using static DopplerHunter.Utilities.FileActionResult;

#nullable disable

namespace DopplerHunter.Tests.Converters
{
    public class StatusToBrushConverterTests
    {
        private static readonly SolidColorBrush SuccessBrush = Brushes.Green;
        private static readonly SolidColorBrush ErrorBrush = Brushes.Red;
        private static readonly SolidColorBrush WarningBrush = Brushes.Yellow;
        private static readonly SolidColorBrush FailedBrush = Brushes.DarkRed;
        private static readonly SolidColorBrush DefaultBrush = Brushes.Black;

        private readonly StatusToBrushConverter _converter;

        public StatusToBrushConverterTests()
        {
            var testResources = new Dictionary<string, Brush>
            {
                ["SuccessBrush"] = SuccessBrush,
                ["ErrorBrush"] = ErrorBrush,
                ["WarningBrush"] = WarningBrush,
                ["FailedBrush"] = FailedBrush,
                ["DefaultBrush"] = DefaultBrush
            };

            _converter = new StatusToBrushConverter(key => testResources.GetValueOrDefault(key));
        }

        [Theory]
        [InlineData(ResponseAction.Succeeded, "SuccessBrush")]
        [InlineData(ResponseAction.Error, "ErrorBrush")]
        [InlineData(ResponseAction.Warning, "WarningBrush")]
        [InlineData(ResponseAction.Failed, "FailedBrush")]
        public void Should_ReturnMappedBrush_WhenStatusIsRecognized(ResponseAction status, string expectedKey)
        {
            // Act
            var result = _converter.Convert(status, typeof(Brush), null, CultureInfo.InvariantCulture);

            // Assert
            var expected = expectedKey switch
            {
                "SuccessBrush" => SuccessBrush,
                "ErrorBrush" => ErrorBrush,
                "WarningBrush" => WarningBrush,
                "FailedBrush" => FailedBrush,
                _ => DefaultBrush
            };

            Assert.Same(expected, result);
        }

        [Fact]
        public void Should_ReturnDefaultBrush_WhenStatusIsUnrecognized()
        {
            // Arrange
            var unknownStatus = (ResponseAction)999;

            // Act
            var result = _converter.Convert(unknownStatus, typeof(Brush), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Same(DefaultBrush, result);
        }

        [Fact]
        public void Should_ReturnDefaultBrush_WhenValueIsNotResponseAction()
        {
            // Act
            var nullResult = _converter.Convert(null, typeof(Brush), null, CultureInfo.InvariantCulture);
            var stringResult = _converter.Convert("invalid", typeof(Brush), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Same(DefaultBrush, nullResult);
            Assert.Same(DefaultBrush, stringResult);
        }

        [Fact]
        public void Should_FallbackToDefaultBrush_WhenSpecificBrushNotFoundInResolver()
        {
            // Arrange: resolver with only DefaultBrush
            var partialResolver = new StatusToBrushConverter(key => key == "DefaultBrush" ? DefaultBrush : null);

            // Act
            var result = partialResolver.Convert(ResponseAction.Succeeded, typeof(Brush), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Same(DefaultBrush, result);
        }

        [Fact]
        public void Should_ThrowNotImplementedException_WhenConvertBackCalled()
        {
            // Assert
            Assert.Throws<NotImplementedException>(() =>
                _converter.ConvertBack(SuccessBrush, typeof(ResponseAction), null, CultureInfo.InvariantCulture));
        }
    }
}
