using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using MidnightLockTwo.Resources;
using System.Diagnostics;

namespace MidnightLockTwo
{

    public partial class MainPage : PhoneApplicationPage
    {

        public MainPage()
        {
            InitializeComponent();
        }

        private async void TestButton_Click(object sender, RoutedEventArgs e)
        {
            WhatsApp.Client client = new WhatsApp.Client();
            await client.Initialize();
            List<Tuple<int, WhatsApp.Sender>> result = await client.GetUnreadMessagesMetaData();
            client.Dispose();
            Debugger.Break();
        }

    }

}