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
            Statement st = await db.PrepareStatementAsync(@"
                SELECT  Conversations.UnreadMessageCount, Conversations.Jid, Conversations.GroupSubject, Conversations.FirstUnreadMessageID, Messages.PushName
                FROM Conversations
                INNER JOIN Messages
                ON Messages.MessageID = Conversations.FirstUnreadMessageID
                WHERE UnreadMessageCount <> 0
            ");
            string allmsg = "";
            while(await st.StepAsync())
            {
                string msg = st.GetIntAt(0).ToString() + " | " +
                    st.GetTextAt(1) + " | " +
                    st.GetTextAt(2) + " | " +
                    st.GetIntAt(3).ToString() + " | " +
                    st.GetTextAt(4);
                allmsg += msg + Environment.NewLine;
                Debug.WriteLine(msg);
            }
            db.Dispose();
            MessageBox.Show(allmsg);
        }

    }

}