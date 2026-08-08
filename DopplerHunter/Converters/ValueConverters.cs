//using System;
//using System.Globalization;
//using System.Windows;
//using System.Windows.Data;

//namespace DopplerHunter.Converters
//{
//    /// <summary>
//    /// Convierte bool a Visibility. True = Visible, False = Collapsed.
//    /// </summary>
//    public class BooleanToVisibilityConverter : IValueConverter
//    {
//        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
//        {
//            if (value is bool boolean)
//                return boolean ? Visibility.Visible : Visibility.Collapsed;
//            return Visibility.Collapsed;
//        }

//        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
//        {
//            if (value is Visibility visibility)
//                return visibility == Visibility.Visible;
//            return false;
//        }
//    }

//    /// <summary>
//    /// Convierte bool inverso a Visibility. False = Visible, True = Collapsed.
//    /// </summary>
//    public class InverseBooleanToVisibilityConverter : IValueConverter
//    {
//        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
//        {
//            if (value is bool boolean)
//                return !boolean ? Visibility.Visible : Visibility.Collapsed;
//            return Visibility.Collapsed;
//        }

//        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
//        {
//            if (value is Visibility visibility)
//                return visibility != Visibility.Visible;
//            return true;
//        }
//    }

//    /// <summary>
//    /// Convierte bool inverso. True -> False, False -> True.
//    /// </summary>
//    public class InverseBooleanConverter : IValueConverter
//    {
//        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
//        {
//            if (value is bool boolean)
//                return !boolean;
//            return true;
//        }

//        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
//        {
//            if (value is bool boolean)
//                return !boolean;
//            return true;
//        }
//    }

//    /// <summary>
//    /// Convierte int a Visibility. 0 = Visible, > 0 = Collapsed.
//    /// </summary>
//    public class ZeroToVisibilityConverter : IValueConverter
//    {
//        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
//        {
//            if (value is int intValue)
//                return intValue == 0 ? Visibility.Visible : Visibility.Collapsed;
//            return Visibility.Collapsed;
//        }

//        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
//        {
//            return 0;
//        }
//    }
//}
