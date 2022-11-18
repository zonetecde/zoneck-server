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
                this.userApp = string.Empty ;
            }
        }

        public class Message
        {
            public Message(string id, string content, MESSAGE_TYPE messageType, string toId = "")
            {
                Id = id;
                Content = content;
                MessageType = messageType;
                ToId = toId;
            }

            public string Id { get; set; }
            public string Content { get; set; }
            public string ToId { get; set; }


            public MESSAGE_TYPE MessageType { get; set; }
        }

        public enum MESSAGE_TYPE
        {
            MESSAGE,
            CONNECTION,
            DISCONNECTION,
            DONNER_ID
        }
    }
}
