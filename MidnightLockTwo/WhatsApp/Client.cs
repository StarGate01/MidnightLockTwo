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

    public class Client : IDisposable
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

        public static readonly Uri 
            FALLBACK_IMG_SINGLE = new Uri(@"/Assets/WhatsApp/default-contact-icon.png", UriKind.Relative),
            FALLBACK_IMG_GROUP = new Uri(@"/Assets/WhatsApp/default-group-icon.png", UriKind.Relative);

        private const string 
            FALLBACK_NAME = "Unknown",
            FALLBACK_FULLNAME = "Unknown Number";

        private static readonly Sender FALLBACK_SENDER = new Sender(FALLBACK_NAME, FALLBACK_FULLNAME, FALLBACK_IMG_SINGLE, Sender.OriginType.Single);

        #endregion

        #region Statements

        private const string 
            UNREAD_MSGS_SQL = @"
                SELECT Conversations.UnreadMessageCount, Conversations.Jid, Conversations.GroupSubject 
                FROM Conversations 
                WHERE UnreadMessageCount <> 0;",
            PERSONAL_INFO_SQL = @"
                SELECT ChatPictures.WaPhotoId , PhoneNumbers.RawPhoneNumber,  UserStatuses.ContactName, UserStatuses.FirstName, UserStatuses.PushName 
                FROM ChatPictures 
                LEFT OUTER JOIN PhoneNumbers 
                ON ChatPictures.Jid = PhoneNumbers.Jid 
                LEFT OUTER JOIN UserStatuses 
                ON ChatPictures.Jid = UserStatuses.Jid 
                WHERE ChatPictures.Jid = ?;";
        private Statement unreadMsgs, personalInfo;

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
            personalInfo = await contactsDatabase.PrepareStatementAsync(PERSONAL_INFO_SQL);
            _clientState = State.Ready;
        }

        public async Task<List<MessagesMetaData>> GetUnreadMessagesMetaData()
        {
            if (_clientState != State.Ready) throw new InvalidOperationException();
            List<MessagesMetaData> messages = new List<MessagesMetaData>();
            unreadMsgs.Reset();
            while (await unreadMsgs.StepAsync())
            {
                string groupName = unreadMsgs.GetTextAt(2);
                Sender sender = await GetPersonalInfo(unreadMsgs.GetTextAt(1), (groupName == null || groupName == "") ? Sender.OriginType.Single : Sender.OriginType.Group);
                if(sender.Origin == Sender.OriginType.Group)
                {
                    sender.Name = groupName;
                    sender.FullName = groupName;
                }
                messages.Add(new MessagesMetaData(unreadMsgs.GetIntAt(0), sender));
            }
            return messages;
        }

        private Task<string> FindRootDir()
        {
            return Task.Run(() =>
            {
                string foundDir = new Func <string> (() =>
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
                })();
                if (foundDir != null)
                {
                    IsolatedStorageSettings.ApplicationSettings[WA_ROOT_DIR_CONF_KEY] = foundDir;
                    IsolatedStorageSettings.ApplicationSettings.Save();
                    return foundDir;
                }
                throw new DirectoryNotFoundException();
            });
        }

        private async Task<Sender> GetPersonalInfo(string jid, Sender.OriginType type)
        {
            if (_clientState != State.Ready) throw new InvalidOperationException();
            personalInfo.Reset();
            personalInfo.BindTextParameterAt(1, jid);
            if(await personalInfo.StepAsync())
            {
                string waPhotoId = personalInfo.GetTextAt(0);
                Uri profileImage = (type == Sender.OriginType.Single) ? FALLBACK_IMG_SINGLE : FALLBACK_IMG_GROUP;
                if (waPhotoId != null && waPhotoId != "") profileImage = new Uri(rootDir + APPDATA_PICTURE_DIR + @"\" + jid + waPhotoId + @"_thumb", UriKind.Absolute);
                if (type == Sender.OriginType.Group) return new Sender(string.Empty, string.Empty, profileImage, type);
                else
                {
                    string[] nameCandidates = new string[] { personalInfo.GetTextAt(3), personalInfo.GetTextAt(4), personalInfo.GetTextAt(1) };
                    string name = nameCandidates[2];
                    if (nameCandidates[0] != null && nameCandidates[0] != "") name = nameCandidates[0];
                    else if (nameCandidates[1] != null && nameCandidates[1] != "") name = nameCandidates[1];
                    string fullName = personalInfo.GetTextAt(2);
                    if (name == null || name == "")
                    {
                        if(fullName == null || fullName == "")
                        {
                            name = FALLBACK_NAME;
                            fullName = FALLBACK_FULLNAME;
                        }
                        else name = fullName;
                    }
                    else if (fullName == null || fullName == "") fullName = name;
                    return new Sender(name, fullName, profileImage, type);
                }
            }
            return FALLBACK_SENDER;
        }

        public void Dispose()
        {
            if(msgDatabase != null) msgDatabase.Dispose();
            if (contactsDatabase != null) contactsDatabase.Dispose();
            _clientState = State.Disposed;
        }

    }

}
