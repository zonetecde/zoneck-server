using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    public class ClassLibrary
    {
        public class LastMessage
        {
            public string userID;
            public string lastMessage;

            public LastMessage(string userID, string lastMessage)
            {
                this.userID = userID;
                this.lastMessage = lastMessage;
            }
        }

        public class User
        {
            public string userID;
            public string userApp;
            public string lastMessage;

            public User(string userID)
            {
                this.userID = userID;
                this.userApp = string.Empty;
            }
        }

        public class Message
        {
            public Message(string id, string content, MESSAGE_TYPE messageType, string toId = "", string args = "")
            {
                Id = id;
                Content = content;
                MessageType = messageType;
                ToId = toId;
                Args = args;
            }

            public string Id { get; set; }
            public string Content { get; set; }
            public string ToId { get; set; }
            public string Args { get; }

            public MESSAGE_TYPE MessageType { get; set; }
        }

        public enum MESSAGE_TYPE
        {
            MESSAGE,
            CONNECTION,
            DISCONNECTION,
            DONNER_ID,
            MESSAGE_BRUTE,
            FILE_UPDATED, // au passé, mais c'est car le client lui recevra ceci lorsqu'un fichié a été modifié par un user
            GET_FILE,
            FILE_DELETED, // au passé, mais c'est car le client lui recevra ceci lorsqu'un fichié a été modifié par un user
            CREATE_FILE,
            LIST_FILE
        }

        public class FileMessage
        {
            public string Path;
            public string Content;
            public bool WarnOtherPeople;

            public FileMessage(string path, bool warnOtherPeople = false, string content = "")
            {
                this.Path = path;
                this.WarnOtherPeople = warnOtherPeople;
                this.Content = content;
            }
        }
    }
}
