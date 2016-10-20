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
using NativeComponent;
using NativeComponent.Notifications;
using Windows.Phone.Notification.Management;

namespace MidnightLockTwo
{

    public partial class MainPage : PhoneApplicationPage
    {

        public MainPage()
        {
            InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await App.MainViewModel.Initialize();
        }

        private void buttonTest_Click(object sender, RoutedEventArgs e)
        {
            //shellChromeAPI.SetShowNowPlayingMediaArt(true);
            //AccessoryManager.EnableAccessoryNotificationTypes((int)AccessoryNotificationType.Toast);
            AccessoryManager.RegisterAccessoryApp();
            AccessoryManager.EnableAccessoryNotificationTypes(65535);
            IReadOnlyDictionary<string, AppNotificationInfo> apps = AccessoryManager.GetApps();
            foreach (KeyValuePair<string, AppNotificationInfo> p in apps) AccessoryManager.EnableNotificationsForApplication(p.Value.Id);
            IAccessoryNotificationTriggerDetails det = null;
            do
            {
                det = AccessoryManager.GetNextTriggerDetails();
            } while (det == null);
            MessageBox.Show(det.GetType().ToString());
            AccessoryManager.ProcessTriggerDetails(det);
            //Windows.Phone.Notification.Management.AccessoryManager.DecreaseVolume(1);
        }
    }

}