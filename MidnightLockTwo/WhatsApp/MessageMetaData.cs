using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidnightLockTwo.WhatsApp
{
    public class MessagesMetaData : INotifyPropertyChanged
    {

        private int _count;
        public int Count
        {
            get
            {
                return _count;
            }
            set
            {
                _count = value;
                RaisePropertyChanged("Count");
            }
        }

        private Sender _sender;
        public Sender Sender
        {
            get
            {
                return _sender;
            }
            set
            {
                _sender = value;
                RaisePropertyChanged("Sender");
            }
        }

        public MessagesMetaData(int count, Sender sender)
        {
           _count = count;
           _sender = sender;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public MessagesMetaData GetCopy()
        {
            MessagesMetaData copy = (MessagesMetaData)this.MemberwiseClone();
            return copy;
        }

    }

}
