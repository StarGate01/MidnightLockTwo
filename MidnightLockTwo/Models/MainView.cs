using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MidnightLockTwo.Models
{
    public class MainView : INotifyPropertyChanged
    {

        private WhatsApp.Client client;

        private List<WhatsApp.MessagesMetaData> _messages;
        public List<WhatsApp.MessagesMetaData> Messages
        {
            get
            {
                return _messages;
            }
            set
            {
                _messages = value;
                RaisePropertyChanged("Messages");
            }
        }

        public MainView()
        {
            if (DesignerProperties.IsInDesignTool)
            {
                PopulateDesignerData();
                return;
            }
            _messages = new List<WhatsApp.MessagesMetaData>();
            client = new WhatsApp.Client();
        }

        public async Task Initialize()
        {
            if (DesignerProperties.IsInDesignTool) return;
            if(client.ClientState != WhatsApp.Client.State.Ready) await client.Initialize();
            await ReloadData();
        }

        public async Task ReloadData()
        {
            if (DesignerProperties.IsInDesignTool) return;
            Messages = await client.GetUnreadMessagesMetaData();
        }

        private void PopulateDesignerData()
        {
            Messages = new List<WhatsApp.MessagesMetaData>()
            {
                { new WhatsApp.MessagesMetaData(3, new WhatsApp.Sender("Max", "Max Mustermann", WhatsApp.Client.FALLBACK_IMG_SINGLE, WhatsApp.Sender.OriginType.Single)) },
                { new WhatsApp.MessagesMetaData(2, new WhatsApp.Sender("Oceans 11", "Oceans 11", WhatsApp.Client.FALLBACK_IMG_GROUP, WhatsApp.Sender.OriginType.Group)) }
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public MainView GetCopy()
        {
            MainView copy = (MainView)this.MemberwiseClone();
            return copy;
        }

    }

}
