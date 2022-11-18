using Newtonsoft.Json;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static ClassLibrary.ClassLibrary;

namespace sck_server
{
    public class ZoneckServer
    {
        static AppServer Server { get; set; }

        private static bool enableDebug = false;

        public ZoneckServer(string IP = "127.0.0.1", int Port = 30000)
        {
            Server = new AppServer();

            var m_Config = new ServerConfig
            {
                Port = Port,
                Mode = SocketMode.Tcp,
                Name = "serveur",
                TextEncoding = "UTF-8",
                Ip = IP,
            };

            // essaye de setup le serveur
            if (!Server.Setup(m_Config))//Setup with listening port
            {
                Console.WriteLine("Failed to setup!");
                Console.ReadKey();
                return;
            }

            // essaye de start le serveur
            if (!Server.Start())
            {
                Console.WriteLine("Failed to start!");
                Console.ReadKey();
                return;
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

            Console.WriteLine("Debug? Y/N");
            if (Console.ReadLine() == "Y")
                enableDebug = true;

            // pour quitter le serveur ou activer debug
            while (Console.ReadKey().KeyChar != 'q')
            {
                Console.WriteLine();
                continue;
            }

            // serveur stoppé
            Server.Stop();

            Console.WriteLine("The Server was stopped!");
            Console.ReadKey();
        }

        /// <summary>
        /// Nouvelle personne connectée au serveur
        /// </summary>
        /// <param name="session">La session de la nouvelle connexion</param>
        static void appServer_NewSessionConnected(AppSession session)
        {          
            // debug log
            if(enableDebug)
                Console.WriteLine("[server-connexion] as " + session.SessionID);

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
                        Console.WriteLine("Envois à " + u.SessionID + " que " + session.SessionID + " s'est connecté.");
                }
            }
        }

        /// <summary>
        /// Personne déconnectée du serveur
        /// </summary>
        /// <param name="session">La personne qui se déconnecte</param>
        /// <param name="aaa">La raison de la déconnexion</param>
        static void appServer_NewSessionClosed(AppSession session, CloseReason aaa)
        {
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
            string disconnectionMessage = session.SessionID + " %disconnection%";
            if (enableDebug)
                Console.WriteLine(DateTime.Now.ToString() + " - " + disconnectionMessage);

            // debug log : nombre de personne connectée sur le serveur
            if (enableDebug)
                Console.WriteLine("utilisateur connecté au serveur " + Server.GetAllSessions().Count());
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
            Message received_message = JsonConvert.DeserializeObject<Message>(message_brute);

            // debug log
            if (enableDebug)
                Console.WriteLine(DateTime.Now.ToString() + " - " + received_message.Id + " pour " + received_message.ToId == string.Empty ? "tout le monde" : received_message.ToId + " > " + received_message.Content);


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
                                Console.WriteLine("Envois d'un message de " + clientSession.SessionID + " à " + u.SessionID + " : " + message_brute);
                        }
                    }
                }
            }
            else
            {
                if (Server.GetSessionByID(received_message.ToId).TrySend(message_brute))
                {
                    if(enableDebug)
                    Console.WriteLine("[MESSAGE PRIVE] de " + received_message.Id + " à " + received_message.ToId);
                }
                else
                {
                    if (enableDebug)
                        Console.WriteLine("pas envoyé, erreur!");
                }
            }
                     
        }
    }
}