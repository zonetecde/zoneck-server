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

        /// <summary>
        /// Créé une instance du serveur
        /// </summary>
        /// <param name="IP">L'adresse IP du serveur</param>    
        /// <param name="Port">Le port d'écoute du serveur</param>    
        /// <param name="_DebugMessage">Reçoit les messages de débogage</param>    
        public ZoneckServer(string IP = "127.0.0.1", int Port = 30000, Action<string> _DebugMessage = null)
        {
            Server = new AppServer();
            DebugMessage = _DebugMessage;

            // Configure le serveur
            var m_Config = new ServerConfig
            {
                Port = Port,
                Mode = SocketMode.Tcp,
                Name = "QLS_Zoneck", // Nom du serveur
                TextEncoding = "UTF-8", // Encoding des données 
                Ip = IP,
                MaxRequestLength = int.MaxValue, // Taille max d'une requête
            };

            // Essaye de Setup le serveur avec le port d'écoute
            if (!Server.Setup(m_Config))
            {
                throw WrongIpOrPortException;
            }

            // Essaye de start le serveur
            if (!Server.Start())
            {
                throw FailedToStartException;
            }

            // Le serveur est lancé 

            // Event : Client connecté
            Server.NewSessionConnected += new SessionHandler<AppSession>(appServer_NewSessionConnected);
            // Event : Client déconnectée
            Server.SessionClosed += appServer_NewSessionClosed;
            // Event : Requête reçu d'un client
            Server.NewRequestReceived += new RequestHandler<AppSession, StringRequestInfo>(appServer_NewRequestReceived);

            enableDebug = DebugMessage != null;

            // Si le serveur est une application console, décommenter ce qui suit :

            //Console.WriteLine("The Server started successfully, press key'q' to stop it! \nIp : " + Server.Config.Ip + "\nPort : " + Server.Config.Port); ;
            //while (Console.ReadKey().KeyChar != 'q')
            //{
            //    Console.WriteLine();
            //    continue;
            //}
        }

        /// <summary>
        /// Stop le serveur
        /// </summary>
        public void StopServer()
        {
            Server.Stop();
        }

        /// <summary>
        /// Un nouveau client s'est connecté au serveur
        /// </summary>
        /// <param name="session">La session de la nouvelle connexion</param>
        static void appServer_NewSessionConnected(AppSession session)
        {
            NumberClient++;

            if (enableDebug)
            {
                DebugMessage(DateTime.Now.ToString() + " - " + "Nouveau client connecté : " + session.SessionID);
                DebugMessage(DateTime.Now.ToString() + " - Nombre de personne(s) connectée(s) au serveur : " + (NumberClient));
            }

            // Le client reçoit son propre Id par l'intermédiaire de ce message. Il est ensuite interprété par le client, et il le stock dans une variable.
            session.Send(JsonConvert.SerializeObject(new Message("serveur", session.SessionID, MESSAGE_TYPE.DONNER_ID))); // Envois l'id au client

            // Informe les autres clients que x.Id s'est connecté
            // Créé le message à envoyer
            Message message_connection = new Message(session.SessionID, string.Empty, MESSAGE_TYPE.CONNECTION);
            string msg = JsonConvert.SerializeObject(message_connection);

            // Pour chaque session sur le serveur
            foreach (var u in Server.GetAllSessions().ToList())
            {
                // Si ce n'est pas le client connecté 
                if (u.SessionID != session.SessionID)
                {
                    // Prévient le client de la connexion de 'session'  
                    u.Send(msg);
                    if (enableDebug)
                        DebugMessage(DateTime.Now.ToString() + " - " + u.SessionID + " sait que " + session.SessionID + " s'est connecté");
                }
            }
        }

        /// <summary>
        /// Un client s'est déconnecté du serveur
        /// </summary>
        /// <param name="session">La session du client qui s'est déconnecté</param>
        /// <param name="reason">La raison de la déconnexion</param>
        static void appServer_NewSessionClosed(AppSession session, CloseReason reason)
        {
            // Retire 1 au compteur de client
            NumberClient--;

            if (enableDebug)
            {
                DebugMessage(DateTime.Now.ToString() + " - Le client " + session.SessionID + " s'est déconnecté");
                DebugMessage(DateTime.Now.ToString() + " - Nombre de personne(s) connectée(s) au serveur : " + (NumberClient));
            }

            // Créer le message de déconnexion à envoyer à toutes les personnes connectées au serveur 
            Message msgDeconnexion = new Message(session.SessionID, string.Empty, MESSAGE_TYPE.DISCONNECTION);
            string msg = JsonConvert.SerializeObject(msgDeconnexion);

            // Pour chaque session sur le serveur
            foreach (var u in Server.GetAllSessions())
            {
                // Si ce n'est pas le client déconnecté 
                if (u.SessionID != session.SessionID)
                {
                    // Prévient le client de la déconnexion de 'session'  
                    u.Send(msg);
                    if (enableDebug)
                        DebugMessage(DateTime.Now.ToString() + " - " + u.SessionID + " sait que " + session.SessionID + " s'est déconnecté");
                }
            }
        }

        /// <summary>
        /// Message reçu d'un client
        /// </summary>
        /// <param name="clientSession">La session du client qui a envoyé un message</param>
        /// <param name="requestInfo">Le message en json, objet du type 'Message'</param>
        static void appServer_NewRequestReceived(AppSession clientSession, StringRequestInfo requestInfo)
        {
            // Récupère le message json
            string message_brute = (requestInfo.Key + " " + requestInfo.Body).Replace("\r\n", string.Empty);

            // Récupère l'objet Message
            Message received_message = JsonConvert.DeserializeObject<Message>(message_brute);

            // Demande de création de fichier sur le serveur
            // Le .Content contient le chemin d'accès au fichier
            if (received_message.MessageType == MESSAGE_TYPE.CREATE_FILE)
            {
                // Si le fichier n'existe pas déjà
                if (!File.Exists("serverfiles/" + received_message.Content))
                {
                    // Créé d'abord le dossier
                    Directory.CreateDirectory("serverfiles/" + Path.GetDirectoryName(received_message.Content)); // créer dossier

                    // Créé ensuite le fichier texte vide
                    File.WriteAllText("serverfiles/" + received_message.Content, ""); // créer fichier
                }

                if (enableDebug)
                    DebugMessage((DateTime.Now.ToString() + " - " + received_message.Id + " created ") + received_message.Content);

            }
            // Demande de supprimer un fichier sur le serveur
            // Le .Content contient un json d'un objet FileMessage contenant le chemin d'accès au fichier 
            // et si on veut que les autres clients soit au courant de la suppression du fichier
            else if (received_message.MessageType == MESSAGE_TYPE.FILE_DELETED)
            {
                // Récupère l'objet FileMessage du .Content
                FileMessage fM = JsonConvert.DeserializeObject<FileMessage>(received_message.Content);

                // Si le fichier existe bien
                if (File.Exists("serverfiles/" + fM.Path))
                {
                    // Supprime le fichier du serveur
                    File.Delete("serverfiles/" + fM.Path);

                    if (enableDebug)
                        DebugMessage((DateTime.Now.ToString() + " - " + received_message.Id + " a supprimé le fichier ") + fM.Path);

                    // Si on veut que les autres clients soit au courant que le fichier a été supprimé
                    if (fM.WarnOtherPeople)
                    {
                        // Envois un message aux autres clients
                        foreach (var u in Server.GetAllSessions().ToList())
                        {
                            // Si ce n'est pas l'utilisateur ayant supprimé le fichier
                            if (u.SessionID != clientSession.SessionID)
                            {
                                // Envois l'alerte au client 'u' que le fichier a été supprimé
                                u.Send(JsonConvert.SerializeObject(new Message(received_message.Id, fM.Path, MESSAGE_TYPE.FILE_DELETED, args: received_message.Args)));

                                if (enableDebug)
                                    DebugMessage(DateTime.Now.ToString() + " - Le client " + u.SessionID + " sait que " + fM.Path + " a été supprimé");
                            }
                        }
                    }
                }
            }
            // Si le client veut recevoir une liste des fichiers présent sur le serveur
            else if (received_message.MessageType == MESSAGE_TYPE.LIST_FILE)
            {
                // Liste tous les fichiers du serveur créé par les clients
                if (!Directory.Exists("serverfiles/"))
                    Directory.CreateDirectory("serverfiles/");

                var files = Directory.GetFiles("serverfiles/", "*.*", SearchOption.AllDirectories);

                // Renvois à la personne un objet de type Message contenant la liste des fichiers séparés par une virgule.
                Message msg = new Message(received_message.Content, String.Join(",", files).Replace("serverfiles/", string.Empty), MESSAGE_TYPE.LIST_FILE,
                    args: received_message.Args);
                Server.GetSessionByID(received_message.Id).TrySend(JsonConvert.SerializeObject(msg));

                if (enableDebug)
                    DebugMessage((DateTime.Now.ToString() + " - " + received_message.Id + " a reçu une liste des fichiers présents sur le serveur"));
            }
            // Si le client veut modifier un fichier
            // Le .Content contient un json d'un objet FileMessage contenant le chemin d'accès au fichier, 
            // le nouveau contenue du fichier a modifié et si on veut que les autres clients soit au courant de la modification du fichier
            else if (received_message.MessageType == MESSAGE_TYPE.FILE_UPDATED)
            {
                // Récupère le message FileMessage
                FileMessage fM = JsonConvert.DeserializeObject<FileMessage>(received_message.Content);

                if (enableDebug)
                    DebugMessage(DateTime.Now.ToString() + " - " + received_message.Id + " a modifié le fichier : " + fM.Path);

                // Si le fichier existe
                if (File.Exists("serverfiles/" + fM.Path))
                    // Le modifie
                    File.WriteAllText("serverfiles/" + fM.Path, fM.Content);
                else
                {
                    // Créé le dossier et le fichier avant son contenue
                    Directory.CreateDirectory("serverfiles/" + Path.GetDirectoryName(fM.Path)); // créer dossier
                    File.WriteAllText("serverfiles/" + fM.Path, fM.Content);
                }

                // Si on veut que les autres clients soit au courant de la modification du fichier
                if (fM.WarnOtherPeople)
                {
                    // envois un message aux autres utilisateurs
                    foreach (var u in Server.GetAllSessions().ToList())
                    {
                        if (u.SessionID != clientSession.SessionID)
                        {
                            // Envois l'information que le fichier a été modifié
                            // .Content est un objet de type FileMessage contenant le chemin d'accès au fichier + son nouveau contenue
                            u.Send(JsonConvert.SerializeObject(new Message(received_message.Id, JsonConvert.SerializeObject(new FileMessage(fM.Path, content: fM.Content)), MESSAGE_TYPE.FILE_UPDATED, args: received_message.Args)));

                            if (enableDebug)
                                DebugMessage(DateTime.Now.ToString() + " - " + u.SessionID + " sait que le fichier " + fM.Path + " a été modifié");
                        }
                    }
                }
            }
            // Si le client veut récupérer le contenue d'un fichier du serveur
            else if (received_message.MessageType == MESSAGE_TYPE.GET_FILE)
            {
                // Si le fichier n'existe pas on le créé
                if (!File.Exists("serverfiles/" + received_message.Content))
                    File.WriteAllText("serverfiles/" + received_message.Content, "");

                // Récupère le contenue du fichier
                string content = File.ReadAllText("serverfiles/" + received_message.Content);

                // Renvois le contenu du fichier au client dans un objet FileMessage
                // fM.Path = fichier
                // fM.Content = son contenue
                Server.GetSessionByID(received_message.Id).TrySend(JsonConvert.SerializeObject(new Message("server", JsonConvert.SerializeObject(new FileMessage(received_message.Content, content: content)), MESSAGE_TYPE.GET_FILE, args:received_message.Args)));

                if (enableDebug)
                    DebugMessage((DateTime.Now.ToString() + " - " + received_message.Id + " a reçu le contenu du fichier ") + received_message.Content);
            }
            else
            {
                if (enableDebug)
                    DebugMessage((DateTime.Now.ToString() + " - Message envoyé de " + received_message.Id + " à ") + received_message.ToId == string.Empty ? "tout le monde" : received_message.ToId + " : " + received_message.Content);

                // Si c'est pour tout le monde ou pour une personne en particulier
                if (String.IsNullOrEmpty(received_message.ToId))
                {
                    // Pour tout le monde 
                    foreach (var u in Server.GetAllSessions().ToList())
                    {
                        if (u.SessionID != clientSession.SessionID)
                        {
                            u.Send(message_brute);
                        }
                    }
                }
                // Message privé
                else
                {
                    if (!Server.GetSessionByID(received_message.ToId).TrySend(message_brute))
                    {
                        // Échoué : l'utilisateur n'est sûrement plus connecté 
                    }
                }
            }
        }
    }
}