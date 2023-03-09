using Newtonsoft.Json;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;
using System;
using System.IO;
using System.Linq;
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
            Message message_connection = new Message(session.SessionID, string.Empty, MESSAGE_TYPE.CONNECTION);
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
                    if (!File.Exists("serverfiles/" + received_message.Content))
                    {
                        Directory.CreateDirectory("serverfiles/" + Path.GetDirectoryName(received_message.Content)); // créer dossier

                        File.WriteAllText("serverfiles/" + received_message.Content, ""); // créer fichier
                    }

                    if (enableDebug)
                        DebugMessage((DateTime.Now.ToString() + " - " + received_message.Id + " created ") + received_message.Content);

                }
                else if (received_message.MessageType == MESSAGE_TYPE.FILE_DELETED)
                {
                    FileMessage fM = JsonConvert.DeserializeObject<FileMessage>(received_message.Content);

                    if (File.Exists("serverfiles/" + fM.path))
                        File.Delete("serverfiles/" + fM.path);

                    if (fM.warnOtherPeople)
                    {
                        // envois un message aux autres utilisateurs
                        foreach (var u in Server.GetAllSessions().ToList())
                        {
                            if (u.SessionID != clientSession.SessionID)
                            {

                                u.Send(JsonConvert.SerializeObject(new Message(received_message.Id, fM.path, MESSAGE_TYPE.FILE_DELETED)));

                                if (enableDebug)
                                    DebugMessage(DateTime.Now.ToString() + " - message sent from " + clientSession.SessionID + " to " + u.SessionID + " : " + "file deleted");

                            }
                        }
                    }

                    if (enableDebug)
                        DebugMessage((DateTime.Now.ToString() + " - " + received_message.Id + " deleted ") + fM.path);
                }
                else if (received_message.MessageType == MESSAGE_TYPE.LIST_FILE)
                {
                    var files = Directory.GetFiles("serverfiles/", "*.*", SearchOption.AllDirectories);

                    Server.GetSessionByID(received_message.Id).TrySend(JsonConvert.SerializeObject(new Message(received_message.Content, String.Join(",", files).Replace("serverfiles/", string.Empty), MESSAGE_TYPE.LIST_FILE)));

                    if (enableDebug)
                        DebugMessage((DateTime.Now.ToString() + " - " + received_message.Id + " listed created files"));
                }
                else if (received_message.MessageType == MESSAGE_TYPE.FILE_UPDATED)
                {
                    FileMessage fM = JsonConvert.DeserializeObject<FileMessage>(received_message.Content);

                    if (File.Exists("serverfiles/" + fM.path))
                        File.WriteAllText("serverfiles/" + fM.path, fM.content);
                    else
                    {
                        Directory.CreateDirectory("serverfiles/" + Path.GetDirectoryName(fM.path)); // créer dossier
                        File.WriteAllText("serverfiles/" + fM.path, fM.content);

                    }

                    if (fM.warnOtherPeople)
                    {
                        // envois un message aux autres utilisateurs
                        foreach (var u in Server.GetAllSessions().ToList())
                        {
                            if (u.SessionID != clientSession.SessionID)
                            {
                                // .content est un objet de type FileMessage contenant le chemin d'accès au fichier + son nouveau contenue
                                u.Send(JsonConvert.SerializeObject(new Message(received_message.Id, JsonConvert.SerializeObject(new FileMessage(fM.path, content: fM.content)), MESSAGE_TYPE.FILE_UPDATED)));

                                if (enableDebug)
                                    DebugMessage(DateTime.Now.ToString() + " - message sent from " + clientSession.SessionID + " to " + u.SessionID + " : " + "file updated");

                            }
                        }
                    }


                    if (enableDebug)
                        DebugMessage((DateTime.Now.ToString() + " - " + received_message.Id + " modified ") + fM.path + " : " + fM.content);
                }
                else if (received_message.MessageType == MESSAGE_TYPE.GET_FILE)
                {
                    // filepath
                    if (!File.Exists("serverfiles/" + received_message.Content))
                        File.WriteAllText("serverfiles/" + received_message.Content, "");

                    string content = File.ReadAllText("serverfiles/" + received_message.Content);
                    // renvois le contenu du fichier au client
                    // id = serveur,filepath
                    Server.GetSessionByID(received_message.Id).TrySend(JsonConvert.SerializeObject(new Message(received_message.Content, content, MESSAGE_TYPE.GET_FILE)));

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