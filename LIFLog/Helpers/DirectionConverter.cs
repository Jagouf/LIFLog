using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using static LIFLog.ViewModel.Hit;

namespace LIFLog.Helpers
{
    public class DirectionToImage : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DirectionEnum direction = (DirectionEnum)value;

            switch (direction)
            {
                case DirectionEnum.incoming:
                    return Application.Current.Resources["appbar_arrow_left"];
                case DirectionEnum.outgoing:
                    return Application.Current.Resources["appbar_arrow_right"];
                default:
                    return Application.Current.Resources["appbar_arrow_right"];
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
