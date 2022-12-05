using Newtonsoft.Json;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static ClassLibrary.ClassLibrary;

namespace sck_server
{
    public class ZoneckServer
    {
        static AppServer Server { get; set; }
        private static Action<string> DebugMessage { get; set; }

        public Exception WrongIpOrPortException { get; }
        public Exception FailedToStartException { get; }

        private static int NumberClient = 0;

        private static bool enableDebug = true;

        public ZoneckServer(string IP = "127.0.0.1", int Port = 30000, Action<string> _DebugMessage = null)
        {
            Server = new AppServer();
            DebugMessage = _DebugMessage;

            var m_Config = new ServerConfig
            {
                Port = Port,
                Mode = SocketMode.Tcp,
                Name = "QLS_Zoneck",
                TextEncoding = "UTF-8",
                Ip = IP,
                MaxRequestLength = int.MaxValue,
               
            };

            // essaye de setup le serveur
            if (!Server.Setup(m_Config))//Setup with listening port
            {
                throw WrongIpOrPortException;
            }

            // essaye de start le serveur
            if (!Server.Start())
            {
                throw FailedToStartException;
            }

            // server start
            Console.WriteLine("The Server started successfully, press key'q' to stop it! \nIp : " + Server.Config.Ip + "\nPort : " + Server.Config.Port); ;

            //      event
            // nouvelle personne connectée
            Server.NewSessionConnected += new SessionHandler<AppSession>(appServer_NewSessionConnected);
            // personne déconnectée
            Server.SessionClosed += appServer_NewSessionClosed;
            // message reçu d'une personne
            Server.NewRequestReceived += new RequestHandler<AppSession, StringRequestInfo>(appServer_NewRequestReceived);

            enableDebug = DebugMessage != null;

            //System.Threading.Thread.Sleep(100000);

            //while (Console.ReadKey().KeyChar != 'q')
            //{
            //    Console.WriteLine();
            //    continue;
            //}
        }

        public void StopServer()
        {
            // serveur stoppé
            Server.Stop();
        }

        /// <summary>
        /// Nouvelle personne connectée au serveur
        /// </summary>
        /// <param name="session">La session de la nouvelle connexion</param>
        static void appServer_NewSessionConnected(AppSession session)
        {
            NumberClient++;
            // debug log
            if (enableDebug)
            {
                DebugMessage(DateTime.Now.ToString() + " - " + "[server-connexion] as " + session.SessionID);
            }

            session.Send(JsonConvert.SerializeObject(new Message("serveur", session.SessionID, MESSAGE_TYPE.DONNER_ID))); // Envois l'id au client

            // Informe les autres client que x.Id s'est connecté
            Message message_connection = new Message(session.SessionID, string.Empty,  MESSAGE_TYPE.CONNECTION);
            string msg = JsonConvert.SerializeObject(message_connection);

            foreach (var u in Server.GetAllSessions().ToList())
            {
                if (u.SessionID != session.SessionID)
                {
                    u.Send(msg);
                    // debug log
                    if (enableDebug)
                        DebugMessage(DateTime.Now.ToString() + " - " + "[connexion-information-sender] to " + u.SessionID + " that " + session.SessionID + " is connected");
                }
            }

            if (enableDebug)
                DebugMessage(DateTime.Now.ToString() + " - [nbre-personne-connecté] - " + (NumberClient));
        }

        /// <summary>
        /// Personne déconnectée du serveur
        /// </summary>
        /// <param name="session">La personne qui se déconnecte</param>
        /// <param name="aaa">La raison de la déconnexion</param>
        static void appServer_NewSessionClosed(AppSession session, CloseReason aaa)
        {
            NumberClient--;

            // Créer le message de déconnexion à envoyer à toutes les personnes connecté au serveur sur la même App
            Message message_disconnection_as = new Message(session.SessionID, string.Empty, MESSAGE_TYPE.DISCONNECTION);

            // Pour chaque session sur le serveur
            foreach (var u in Server.GetAllSessions())
            {
                // si ce n'est pas la personne déconnecté
                if (u.SessionID != session.SessionID)
                {
                    // le prévient que x s'est déconnecté
                
                    u.Send(JsonConvert.SerializeObject(message_disconnection_as));
                
                    
                }
            }

            // debug log
            string disconnectionMessage = "[disconnection] of " + session.SessionID;
            if (enableDebug)
                DebugMessage(DateTime.Now.ToString() + " - " + disconnectionMessage);

            // debug log : nombre de personne connectée sur le serveur
            if (enableDebug)
                DebugMessage(DateTime.Now.ToString() + " - [nbre-personne-connecté] - " + NumberClient);
        }

        /// <summary>
        /// Message reçu d'un client
        /// </summary>
        /// <param name="clientSession">La session du client qui a envoyé un message</param>
        /// <param name="requestInfo">Le message</param>
        static void appServer_NewRequestReceived(AppSession clientSession, StringRequestInfo requestInfo)
        {
            // récupère le message 
            string message_brute = (requestInfo.Key + " " + requestInfo.Body).Replace("\r\n", string.Empty);
         
            if (!message_brute.Contains("@[B]"))
            {
                Message received_message = JsonConvert.DeserializeObject<Message>(message_brute);

                if (received_message.MessageType == MESSAGE_TYPE.CREATE_FILE)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(received_message.Content));
                    if (enableDebug)
                        DebugMessage((DateTime.Now.ToString() + " - " + received_message.Id + " created ") + received_message.Content);

                }
                else if (received_message.MessageType == MESSAGE_TYPE.DELETE_FILE)
                {
                    File.Delete(received_message.Content);
                    if (enableDebug)
                        DebugMessage((DateTime.Now.ToString() + " - " + received_message.Id + " deleted ") + received_message.Content);
                }
                else if (received_message.MessageType == MESSAGE_TYPE.MODIFY_FILE)
                {
                    // filepath,content
                    string fPath = received_message.Content.Split(new[] { ',' }, 2)[0];
                    string content = received_message.Content.Split(new[] { ',' }, 2)[1];
                    File.WriteAllText(fPath, content);

                    if (enableDebug)
                        DebugMessage((DateTime.Now.ToString() + " - " + received_message.Id + " modified ") + fPath + " : " + content);
                }
                else if (received_message.MessageType == MESSAGE_TYPE.GET_FILE)
                {
                    // filepath
                    File.ReadAllText(received_message.Content);

                    if (enableDebug)
                        DebugMessage((DateTime.Now.ToString() + " - " + received_message.Id + " get ") + received_message.Content + "'s file content");
                }
                else
                {
                    // debug log
                    if (enableDebug)
                        DebugMessage((DateTime.Now.ToString() + " - [message-send] from " + received_message.Id + " to ") + received_message.ToId == string.Empty ? "everyone" : received_message.ToId + " : " + received_message.Content);


                    // si c'est pour tout le monde ou pour une personne en particulier
                    if (String.IsNullOrEmpty(received_message.ToId))
                    {
                        foreach (var u in Server.GetAllSessions().ToList())
                        {
                            if (u.SessionID != clientSession.SessionID)
                            {
                                if (received_message.ToId == string.Empty || received_message.ToId == u.SessionID) // gère l'envois de tt le monde ou private message
                                {
                                    u.Send(message_brute);

                                    if (enableDebug)
                                        DebugMessage(DateTime.Now.ToString() + " - [private-message-send] from " + clientSession.SessionID + " to " + u.SessionID + " : " + message_brute);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (Server.GetSessionByID(received_message.ToId).TrySend(message_brute))
                        {
                            if (enableDebug)
                                DebugMessage(DateTime.Now.ToString() + " - [private-message] from " + received_message.Id + " to " + received_message.ToId);
                        }
                        else
                        {

                        }
                    }
                }
            }
            else
            {
                // message brute
                if (enableDebug)
                    DebugMessage(DateTime.Now.ToString() + " - [private-brute-message-send] from " + clientSession.SessionID + " to " + message_brute.Split(',').Last().Trim());
                Server.GetAllSessions().ToList().FirstOrDefault(x => x.SessionID == message_brute.Split(',').Last().Trim()).Send(message_brute); 
            }
                     
        }
    }
}