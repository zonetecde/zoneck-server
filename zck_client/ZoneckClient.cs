using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ClassLibrary;
using static ClassLibrary.ClassLibrary;

namespace zck_client
{
    public class ZoneckClient
    {
        private Socket SocketClient { get; set; }
        public string MyId { get; set; }
        internal Action<Message> Receive { get; }

        public ZoneckClient(string appName, string ip, int port, Action<Message> Receive)
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

            // Envois au serveur pour avoir son ID


            this.Receive = Receive;
        }

        /// <summary>
        /// Envois un message au serveur
        /// </summary>
        /// <param name="str">Le message à envoyer</param>
        public void Send(string str, string toId = "", bool brute = false)
        {
            // id > message 
            string msg = brute ? "@[B]"  + str + "\r\n" : JsonConvert.SerializeObject(new Message(MyId, str, MESSAGE_TYPE.MESSAGE, toId)) + "\r\n";
            //Receive(new Message(ConnetionId, str, AppName, MESSAGE_TYPE.MESSAGE));

            var buffter = Encoding.UTF8.GetBytes(msg);
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

                try
                {
                    var effective = SocketClient.Receive(buffer);

                    var message = Encoding.UTF8.GetString(buffer, 0, effective);

                    if (!String.IsNullOrEmpty(message))
                    {
                        if (!message.Contains("@[B]"))
                        {
                            Message received_message = JsonConvert.DeserializeObject<Message>(message);

                            // pour id
                            if (received_message.MessageType == MESSAGE_TYPE.DONNER_ID)
                                MyId = received_message.Content;

                            Receive(received_message);
                        }
                        else
                        {
                            Receive(new Message(String.Empty, message.Remove(0, 4), MESSAGE_TYPE.MESSAGE_BRUTE));
                        }

                    }
                }
                catch
                { }
                
            }
        }
    }
}