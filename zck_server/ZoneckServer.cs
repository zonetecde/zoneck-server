﻿using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;

namespace sck_server
{
    public class ZoneckServer
    {
        static AppServer Server { get; set; }

        private static List<User> usersConnected = new List<User>();

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

            // pour quitter le serveur
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
            // envois son adresse personne à la personne connecté pour qu'elle puisse l'enregistrer dans une variable (client)
            session.Send("[Server-connexion] as " + session.SessionID);

            // debug log
            Console.WriteLine("[Server-connexion] as " + session.SessionID);

            // ajoute la connexion à la liste des personnes connectées au serveur
            usersConnected.Add(new User(session.SessionID));
        }

        /// <summary>
        /// Personne déconnectée du serveur
        /// </summary>
        /// <param name="session">La personne qui se déconnecte</param>
        /// <param name="aaa">La raison de la déconnexion</param>
        static void appServer_NewSessionClosed(AppSession session, CloseReason aaa)
        {
            // Créer le message de déconnexion à envoyer à toutes les personnes connecté au serveur sur la même App
            string disconnectionMessage = session.SessionID + " %disconnection%";

            // Pour chaque session sur le serveur
            foreach (var u in Server.GetAllSessions())
            {
                // si ce n'est pas la personne déconnecté
                if (u.SessionID != session.SessionID)
                    // si il appartient à la même App
                    if (usersConnected.FirstOrDefault(x => x.userID == session.SessionID).userApp ==
                        usersConnected.FirstOrDefault(x => x.userID == u.SessionID).userApp)
                        // le prévient que x s'est déconnecté
                        u.Send(disconnectionMessage);
            }

            // debug log
            Console.WriteLine(DateTime.Now.ToString() + " - " + disconnectionMessage);

            // enlève la personne de la liste des users connectés au serveur
            usersConnected.RemoveAll(x => x.userID == session.SessionID);

            // debug log : nombre de personne connectée sur le serveur
            Console.WriteLine("utilisateur connecté au serveur " + usersConnected.Count);
        }

        /// <summary>
        /// Message reçu d'un client
        /// </summary>
        /// <param name="session">La session du client qui a envoyé un message</param>
        /// <param name="requestInfo">Le message</param>
        static void appServer_NewRequestReceived(AppSession session, StringRequestInfo requestInfo)
        {
            // recupère le message au format :
            // [AppName] userId > message
            string message = (requestInfo.Key + " " + requestInfo.Body);

            // debug log
            Console.WriteLine(DateTime.Now.ToString() + " - " + message);

            // set ce message comme étant le dernier que cette personne est envoyé
            usersConnected.FirstOrDefault(x => x.userID == session.SessionID).lastMessage = message;

            // si c'est un message de connexion
            if (message.Contains("%connection%"))
            {
                // on récupère le nom de l'application
                string appName = message.Substring(
                    0,
                    message.IndexOf("]") + 1);

                // on attribue le nom de l'application à l'utilisateur qui s'est connecté
                usersConnected.FirstOrDefault(x => x.userID == session.SessionID).userApp = appName;

                // Enlève du message le nom de l'app, il n'y en a plus besoin.
                // 3 car [0APP]1 2id
                message = message.Remove(0, appName.Length + 3);
            }
            else if (message.Contains("%last_message%")) // si c'est une demande de %last_message% de tous les users
            {
                // Pour chaque utilisateur sur le serveur
                foreach (var u in Server.GetAllSessions().ToList())
                {
                    // Si il appartient à la même App et que ce n'est pas l'envoyeur
                    if (u.SessionID != session.SessionID)
                        if (usersConnected.FirstOrDefault(x => x.userID == session.SessionID).userApp ==
                                usersConnected.FirstOrDefault(x => x.userID == u.SessionID).userApp)
                            // envois à l'utilisateur en réponse le statut de la personne x sur sa dernière activité 
                            session.Send(usersConnected.FirstOrDefault(x => x.userID == u.SessionID).lastMessage + " %last_message%");
                }

                return;
            }

            // Si c'est un message normal - ou de connexion - on l’envoi à tous les users de la même app
            foreach (var u in Server.GetAllSessions().ToList())
            {
                if (u.SessionID != session.SessionID)
                    if (usersConnected.FirstOrDefault(x => x.userID == session.SessionID).userApp ==
                                usersConnected.FirstOrDefault(x => x.userID == u.SessionID).userApp)
                        u.Send(message);
            }
        }
    }

    internal class User
    {
        internal string userID;
        internal string userApp;
        internal string lastMessage;

        public User(string userID)
        {
            this.userID = userID;
            this.userApp = userApp;
        }
    }
}