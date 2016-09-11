using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace MidnightLockTwo.WhatsApp
{

    public class Sender : INotifyPropertyChanged
    {

        public enum OriginType
        {
            Single,
            Group
        }

        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }

        private string _fullName;
        public string FullName
        {
            get
            {
                return _fullName;
            }
            set
            {
                _fullName = value;
                RaisePropertyChanged("FullName");
            }
        }

        private OriginType _origin;
        public OriginType Origin
        {
            get
            {
                return _origin;
            }
            set
            {
                _origin = value;
                RaisePropertyChanged("Origin");
            }
        }

        private Uri _profilePicture;
        public Uri ProfilePicture
        {
            get
            {
                return _profilePicture;
            }
            set
            {
                _profilePicture = value;
                RaisePropertyChanged("ProfilePicture");
            }
        }

        public Sender(string name, string fullName, Uri profilePicture, OriginType origin)
        {
            _name = name;
            _fullName = fullName;
            _origin = origin;
            _profilePicture = profilePicture;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public Sender GetCopy()
        {
            Sender copy = (Sender)this.MemberwiseClone();
            return copy;
        }

    }

    public class GroupVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((Sender.OriginType)value) == Sender.OriginType.Group ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) { return null; }

    }

    public class SingleVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((Sender.OriginType)value) == Sender.OriginType.Single ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) { return null; }

    }


}
