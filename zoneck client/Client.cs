using System.Net;
using System.Net.Sockets;
using System.Text;

namespace zoneck_client
{
    public class zoneck_client
    {
        private Socket SocketClient { get; set; }
        internal static string ConnetionId { get; set; }
        public string AppName { get; }
        internal Action<Message> Receive { get; }

        internal zoneck_client(string appName, string ip, int port, Action<Message> Receive)
        {
            //Create an instance
            SocketClient = new Socket(SocketType.Stream, ProtocolType.Tcp);
            IPAddress _ip = IPAddress.Parse(ip);
            IPEndPoint point = new IPEndPoint(_ip, port);
            //Make connection
            SocketClient.Connect(point);

            //Receive messages from the server continuously
            Thread thread = new Thread(_Receive);
            thread.IsBackground = true;
            thread.Start();

            // le nom de l'application qui précède les messages [APP] id > message 
            AppName = "[" + appName + "]";

            this.Receive = Receive;
        }

        /// <summary>
        /// Envois un message au serveur
        /// </summary>
        /// <param name="str">Le message à envoyer</param>
        internal void Send(string str)
        {
            // [APP NAME] id > message 
            var buffter = Encoding.UTF8.GetBytes(AppName + " " + ConnetionId + " > " + str + "\r\n");
            var temp = SocketClient.Send(buffter);
        }

        private void _Receive()
        {
            while (true)
            {
                //Get the message sent
                byte[] buffer = new byte[1024 * 1024 * 2];
                var effective = SocketClient.Receive(buffer);


                var str = Encoding.UTF8.GetString(buffer, 0, effective);

                if (!String.IsNullOrEmpty(str))
                {
                    if (str.Contains(AppName) || str.Contains("[server-connexion]")) // [server-connexion] est uniquement envoyé à la personne qui s'est connecté 
                    {
                        if (!str.Contains("[server-connexion]"))
                            str = str.Remove(0, (AppName).Length + 1); // On sait que le message vient de l'application là

                        if (str.Contains(" %last_message%")) // si c'est un last message
                        {
                            Receive(
                            new Message(str.Substring(0, str.IndexOf(" %last_message%", StringComparison.Ordinal)), String.Empty,
                            AppName
                            , MESSAGE_TYPE.LAST_MESSAGE)
                            );
                        }
                        else if (str.Contains(" %connection%")) // si c'est une connexion
                        {
                            Receive(
                            new Message(str.Substring(0, str.IndexOf(" %connection%", StringComparison.Ordinal)), String.Empty,
                            AppName
                            , MESSAGE_TYPE.CONNECTION)
                            );
                        }
                        else if (str.Contains(" %disconnection%")) // si c'est une deconnexion 
                        {
                            Receive(
                            new Message(str.Substring(0, str.IndexOf(" %disconnection%", StringComparison.Ordinal)), String.Empty, AppName
        , MESSAGE_TYPE.DISCONNECTION)
                            );
                        }
                        else if (str.Contains(" > ")) // si c'est un message
                        {
                            Receive(
                                new Message(str.Substring(0, str.IndexOf(" > ", StringComparison.Ordinal)), // [id] > message
                                str.Substring(str.IndexOf(" > ") + " > ".Length) // id > [message]
                                    .Replace("\r\n", string.Empty), AppName
        , MESSAGE_TYPE.MESSAGE)
                                );
                        }
                        else // si c'est pour informer de son id de session et informer ensuite le serveur que nous sommes de l'app
                        {
                            ConnetionId = str.Substring(str.IndexOf("[server-connexion] as ") + "[server-connexion] as ".Length).Replace("\r\n", string.Empty);

                            var buffter = Encoding.UTF8.GetBytes(AppName + " " + ConnetionId + " %connection%" + "\r\n");
                            var temp = SocketClient.Send(buffter);
                        }
                    }
                }
            }
        }
    }

    internal class Message
    {
        public Message(string id, string content, string appName, MESSAGE_TYPE messageType)
        {
            Id = id;
            Content = content;
            MessageType = messageType;
            AppName = appName;
        }

        internal string Id { get; set; }
        internal string Content { get; set; }
        internal string AppName { get; set; }

        internal MESSAGE_TYPE MessageType { get; set; }
    }

    internal enum MESSAGE_TYPE
    {
        MESSAGE,
        CONNECTION,
        DISCONNECTION,
        LAST_MESSAGE
    }
}
