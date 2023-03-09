namespace zck_client
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
            DONNER_ID,
            MESSAGE_BRUTE,
            FILE_UPDATED,
            GET_FILE,
            FILE_DELETED,
            CREATE_FILE,
            LIST_FILE
        }

        public class FileMessage
        {
            public string path;
            public string content;
            public bool warnOtherPeople;

            public FileMessage(string path, bool warnOtherPeople, string content = "")
            {
                this.path = path;
                this.warnOtherPeople = warnOtherPeople;
                this.content = content;
            }
        }
    }
}
