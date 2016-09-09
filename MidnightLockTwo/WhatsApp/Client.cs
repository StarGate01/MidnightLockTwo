using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLiteWinRT;
using System.IO;
using System.IO.IsolatedStorage;
using Windows.Storage;

namespace MidnightLockTwo.WhatsApp
{

    class Client : IDisposable
    {

        public enum State
        {
            UnInitialized,
            Error,
            Ready,
            Disposed
        }

        private State _clientState;
        public State ClientState { get { return _clientState; } }

        #region Constants

        private const string 
            WA_PACKAGE_ID = "WhatsApp", 
            WA_ROOT_DIR_CONF_KEY = "WARootDir", 
            APPDATA_ROOT_DIR = @"C:\Data\Users\DefApps\APPDATA\Local\Packages", 
            FIRST_TRY_DIR = APPDATA_ROOT_DIR + @"\5319275A.WhatsApp_cv1g1gvanyjgm", 
            APPDATA_DB_MSG_FILE = @"\LocalState\messages.db", 
            APPDATA_DB_CONTACTS_FILE = @"\LocalState\contacts.db",
            APPDATA_PICTURE_DIR = @"\LocalState\profilePictures";

        #endregion

        #region Statements

        private const string 
            UNREAD_MSGS_SQL = @"
                SELECT Conversations.UnreadMessageCount, Conversations.Jid, Conversations.GroupSubject, Messages.PushName
                FROM Conversations
                INNER JOIN Messages
                ON Messages.MessageID = Conversations.FirstUnreadMessageID
                WHERE UnreadMessageCount <> 0;",
            PHOTO_ID = @"
                SELECT ChatPictures.WaPhotoId 
                FROM ChatPictures 
                WHERE ChatPictures.Jid = ?;";
        private Statement unreadMsgs, photoId;

        #endregion

        private Database msgDatabase, contactsDatabase;
        private string rootDir;

        public Client()
        {
            _clientState = State.UnInitialized;
        }

        public async Task Initialize()
        {
            _clientState = State.Error;
            rootDir = await FindRootDir();
#if DEBUG
            msgDatabase = new Database(await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"Test\messages.db"));
            contactsDatabase = new Database(await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"Test\contacts.db"));
#else
            msgDatabase = new Database(await StorageFile.GetFileFromPathAsync(rootDir + APPDATA_DB_MSG_FILE));
            contactsDatabase = new Database(await StorageFile.GetFileFromPathAsync(rootDir + APPDATA_DB_CONTACTS_FILE));
#endif
            await msgDatabase.OpenAsync(SqliteOpenMode.OpenRead);
            unreadMsgs = await msgDatabase.PrepareStatementAsync(UNREAD_MSGS_SQL);
            await contactsDatabase.OpenAsync(SqliteOpenMode.OpenRead);
            photoId = await contactsDatabase.PrepareStatementAsync(PHOTO_ID);
            _clientState = State.Ready;
        }

        public async Task<List<Tuple<int, Sender>>> GetUnreadMessagesMetaData()
        {
            if (_clientState != State.Ready) throw new Exception("Object is not initialized!");
            List<Tuple<int, Sender>> messages = new List<Tuple<int, Sender>>();
            unreadMsgs.Reset();
            while (await unreadMsgs.StepAsync())
            {
                string name = unreadMsgs.GetTextAt(3);
                string groupName = unreadMsgs.GetTextAt(2);
                if (groupName != null) name = groupName;
                messages.Add(new Tuple<int, Sender>(unreadMsgs.GetIntAt(0),
                    new Sender(name, groupName == null ? Sender.OriginType.Single : Sender.OriginType.Group,
                        await GetThumbProfilePicture(unreadMsgs.GetTextAt(1)))));
            }
            return messages;
        }

        private Task<string> FindRootDir()
        {
            return Task.Run(() =>
            {
                Func<string> yieldDir = () =>
                {
                    if (IsolatedStorageSettings.ApplicationSettings.Contains(WA_ROOT_DIR_CONF_KEY))
                    {
                        string confDir = (string)IsolatedStorageSettings.ApplicationSettings[WA_ROOT_DIR_CONF_KEY];
                        if (Directory.Exists(confDir)) return confDir;
                    }
                    string dirToTry = FIRST_TRY_DIR;
                    if (Directory.Exists(dirToTry)) return dirToTry;
                    dirToTry = Directory.GetFiles(APPDATA_ROOT_DIR).FirstOrDefault(p => p.Contains(WA_PACKAGE_ID));
                    return dirToTry;
                };
                string foundDir = yieldDir();
                if (foundDir != null)
                {
                    IsolatedStorageSettings.ApplicationSettings[WA_ROOT_DIR_CONF_KEY] = foundDir;
                    IsolatedStorageSettings.ApplicationSettings.Save();
                    return foundDir;
                }
                throw new Exception("No WhatsApp package found!");
            });
        }

        private async Task<string> GetThumbProfilePicture(string jid)
        {
            if (_clientState != State.Ready) throw new Exception("Object is not initialized!");
            photoId.Reset();
            photoId.BindTextParameterAt(1, jid);
            if(await photoId.StepAsync())
            {
                string waPhotoId = photoId.GetTextAt(0);
                if (waPhotoId != null) return rootDir + APPDATA_PICTURE_DIR + @"\" + jid + waPhotoId + @"_thumb";
            }
            return null;
        }

        public void Dispose()
        {
            if(msgDatabase != null) msgDatabase.Dispose();
            if (contactsDatabase != null) contactsDatabase.Dispose();
            _clientState = State.Disposed;
        }

    }

}
