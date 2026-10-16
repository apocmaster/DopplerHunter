using DopplerHunter.Utilities;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using static DopplerHunter.Utilities.FileActionResult;

namespace DopplerHunter.Converters
{
    public class StatusToBrushConverter : IValueConverter
    {
        #region Constants and Fields

        private const string DefaultBrushKey = "DefaultBrush";

        private static readonly IReadOnlyDictionary<ResponseAction, string> StatusResourceKeys =
            new Dictionary<ResponseAction, string>
            {
                [ResponseAction.Succeeded] = "SuccessBrush",
                [ResponseAction.Error] = "ErrorBrush",
                [ResponseAction.Warning] = "WarningBrush",
                [ResponseAction.Failed] = "FailedBrush"
            };

        private readonly Func<string, Brush?> _brushResolver;

        #endregion

        #region Constructors

        public StatusToBrushConverter() : this(ResolveApplicationResource)
        {
        }

        public StatusToBrushConverter(Func<string, Brush?> brushResolver)
        {
            _brushResolver = brushResolver ?? throw new ArgumentNullException(nameof(brushResolver));
        }

        #endregion

        #region IValueConverter Implementation

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ResponseAction status)
            {
                return GetBrushForStatus(status);
            }

            return GetBrush(DefaultBrushKey);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Helper Methods

        private Brush GetBrushForStatus(ResponseAction status)
        {
            string resourceKey = GetResourceKey(status);
            return GetBrush(resourceKey);
        }

        private static string GetResourceKey(ResponseAction status)
        {
            return StatusResourceKeys.TryGetValue(status, out var resourceKey)
                ? resourceKey
                : DefaultBrushKey;
        }

        private Brush GetBrush(string resourceKey)
        {
            return _brushResolver(resourceKey)
                ?? _brushResolver(DefaultBrushKey)
                ?? Brushes.Black;
        }

        private static Brush? ResolveApplicationResource(string key)
        {
            return Application.Current?.Resources[key] as Brush;
        }

        #endregion
    }
}
