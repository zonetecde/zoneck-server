using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace zck_client
{
    public class ZoneckClient
    {
        private Socket SocketClient { get; set; }
        internal static string ConnetionId { get; set; }
        public string AppName { get; }
        internal Action<Message> Receive { get; }

        internal ZoneckClient(string appName, string ip, int port, Action<Message> Receive)
        {
            // Créer une instance du SocketClient
            SocketClient = new Socket(SocketType.Stream, ProtocolType.Tcp);
            IPAddress _ip = IPAddress.Parse(ip);
            IPEndPoint point = new IPEndPoint(_ip, port);

            // Connexion
            SocketClient.Connect(point);

            // Thread pour recevoir les messages du serveur en continu
            Thread thread = new Thread(_Receive);
            thread.IsBackground = true;
            thread.Start();

            // Le nom de l'application qui précède le message de connexion
            AppName = "[" + appName + "]";

            this.Receive = Receive;
        }

        /// <summary>
        /// Envois un message au serveur
        /// </summary>
        /// <param name="str">Le message à envoyer</param>
        internal void Send(string str)
        {
            // id > message 
            var buffter = Encoding.UTF8.GetBytes(ConnetionId + " > " + str + "\r\n");
            var temp = SocketClient.Send(buffter);
        }

        /// <summary>
        /// Reçois un message du serveur
        /// </summary>
        private void _Receive()
        {
            while (true)
            {
                // Recupère le message reçu
                byte[] buffer = new byte[1024 * 1024 * 2];
                var effective = SocketClient.Receive(buffer);
                var message = Encoding.UTF8.GetString(buffer, 0, effective);

                // Si le message n'est pas vide
                if (!String.IsNullOrEmpty(message))
                {

                    // Si c'est pour informer de son Id de session et informer ensuite le serveur que nous sommes de l'App (nouvelle connexion)
                    if (message.Contains("[server-connexion]"))
                    {
                        ConnetionId = message.Substring(message.IndexOf("[server-connexion] as ") + "[server-connexion] as ".Length).Replace("\r\n", string.Empty);

                        var buffter = Encoding.UTF8.GetBytes(AppName + " " + ConnetionId + " %connection%" + "\r\n");
                        var temp = SocketClient.Send(buffter);
                    }
                    // Si le message est une réponse à une demande de %last_message% 
                    else if (message.Contains(" %last_message%")) // si c'est un last message
                    {
                        Receive(
                        new Message(message.Substring(0, message.IndexOf(" %last_message%", StringComparison.Ordinal)), String.Empty,
                        AppName
                        , MESSAGE_TYPE.LAST_MESSAGE)
                        );
                    }
                    // Si le message est une information d'une nouvelle connexion
                    else if (message.Contains(" %connection%"))
                    {
                        Receive(
                        new Message(message.Substring(0, message.IndexOf(" %connection%", StringComparison.Ordinal)), String.Empty,
                        AppName
                        , MESSAGE_TYPE.CONNECTION)
                        );
                    }
                    // Si le message est une information d'une déconnexion au serveur
                    else if (message.Contains(" %disconnection%"))
                    {
                        Receive(
                        new Message(message.Substring(0, message.IndexOf(" %disconnection%", StringComparison.Ordinal)), String.Empty, AppName
                            , MESSAGE_TYPE.DISCONNECTION)
                        );
                    }
                    // Si c'est un message normal
                    else if (message.Contains(" > "))
                    {
                        Receive(
                            new Message(message.Substring(0, message.IndexOf(" > ", StringComparison.Ordinal)), // [id] > message
                            message.Substring(message.IndexOf(" > ") + " > ".Length) // id > [message]
                                .Replace("\r\n", string.Empty), AppName
                                    , MESSAGE_TYPE.MESSAGE)
                            );
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
