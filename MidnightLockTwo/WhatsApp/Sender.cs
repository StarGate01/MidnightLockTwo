using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public Sender(string name, OriginType origin, Uri profilePicture)
        {
            _name = name;
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

}
