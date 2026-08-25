using DopplerHunter.Utilities;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using static DopplerHunter.Utilities.FileActionResult;

namespace DopplerHunter.Converters
{
    public class StatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ResponseAction status)
            {
                return status switch
                {
                    ResponseAction.Succeeded => (System.Windows.Media.Brush)Application.Current.Resources["SuccessBrush"],
                    ResponseAction.Error => (System.Windows.Media.Brush)Application.Current.Resources["ErrorBrush"],
                    ResponseAction.Warning => (System.Windows.Media.Brush)Application.Current.Resources["WarningBrush"],
                };
            }
            return (System.Windows.Media.Brush)Application.Current.Resources["DefaultBrush"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
