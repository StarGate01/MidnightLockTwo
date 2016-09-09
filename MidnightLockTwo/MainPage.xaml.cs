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
using SQLiteWinRT;
using Windows.Storage;
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
            StorageFolder dir = await StorageFolder.GetFolderFromPathAsync(@"C:\Data\Users\DefApps\APPDATA\Local\Packages");
            StorageFolder wadir = (await dir.GetFoldersAsync()).First(p => p.Path.Contains("WhatsApp"));
            StorageFile dbfile = await StorageFile.GetFileFromPathAsync(System.IO.Path.Combine(wadir.Path, @"LocalState\messages.db"));
            Database db = new Database(dbfile);
            await db.OpenAsync(SqliteOpenMode.OpenRead);
            Statement st = await db.PrepareStatementAsync("SELECT * FROM ContactVCards");
            while(await st.StepAsync())
            {
                int msgid = st.GetIntAt(2);
                Debug.WriteLine(msgid.ToString());
            }
            db.Dispose();
        }

    }

}