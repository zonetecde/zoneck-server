using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;

namespace zoneck_server
{
    public class Server
    {
        static AppServer zoneck_server { get; set; }
        static string IP { get; set; } = "127.0.0.1";
        static int Port { get; set; } = 30_000;

        private static List<User> usersConnected = new List<User>();

        static void Main(string[] args)
        {
            zoneck_server = new AppServer();

            var m_Config = new ServerConfig
            {
                Port = Port,
                Mode = SocketMode.Tcp,
                Name = "serveur",
                TextEncoding = "UTF-8",
                Ip = IP,
            };


            //Setup the appServer
            if (!zoneck_server.Setup(m_Config))//Setup with listening port
            {
                Console.WriteLine("Failed to setup!");
                Console.ReadKey();
                return;
            }

            //Try to start the appServer
            if (!zoneck_server.Start())
            {
                Console.WriteLine("Failed to start!");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("The server started successfully, press key'q' to stop it! \nIp : " + zoneck_server.Config.Ip + "\nPort : " + zoneck_server.Config.Port); ;

            //1.
            zoneck_server.NewSessionConnected += new SessionHandler<AppSession>(appServer_NewSessionConnected);
            zoneck_server.SessionClosed += appServer_NewSessionClosed;

            //2.
            zoneck_server.NewRequestReceived += new RequestHandler<AppSession, StringRequestInfo>(appServer_NewRequestReceived);

            while (Console.ReadKey().KeyChar != 'q')
            {
                Console.WriteLine();
                continue;
            }

            //Stop the appServer
            zoneck_server.Stop();

            Console.WriteLine("The server was stopped!");
            Console.ReadKey();
        }

        //1.
        static void appServer_NewSessionConnected(AppSession session)
        {
            session.Send("[server-connexion] as " + session.SessionID);

            Console.WriteLine("[server-connexion] as " + session.SessionID);

            usersConnected.Add(new User(session.SessionID));
        }

        static void appServer_NewSessionClosed(AppSession session, CloseReason aaa)
        {
            string disconnectionMessage = usersConnected.FirstOrDefault(x => x.userID == session.SessionID).userApp + " " + session.SessionID + " %disconnection%";

            foreach (var u in zoneck_server.GetAllSessions())
            {
                if (u.SessionID != session.SessionID)
                    u.Send(disconnectionMessage);
            }

            Console.WriteLine(DateTime.Now.ToString() + " - " + disconnectionMessage);

            usersConnected.RemoveAll(x => x.userID == session.SessionID);

            Console.WriteLine("utilisateur connecté au serveur " + usersConnected.Count);
        }

        //2.
        static void appServer_NewRequestReceived(AppSession session, StringRequestInfo requestInfo)
        {
            string message = (requestInfo.Key + " " + requestInfo.Body);

            int pFrom = message.IndexOf("] ") + "] ".Length;
            int pTo = message.LastIndexOf(" %");

            string userId = message.Substring(pFrom, pTo - pFrom);

            Console.WriteLine(DateTime.Now.ToString() + " - " + message);
            //session.Send(requestInfo.Key);

            usersConnected.FirstOrDefault(x => x.userID == userId).lastMessage = message;

            if (message.Contains("%connection%"))
            {


                string appName = message.Substring(
                    0,
                    message.IndexOf("]") + 1);

                usersConnected.FirstOrDefault(x => x.userID == userId).userApp = appName;
            }
            else if(message.Contains("%last_message%")) // send le last message de toutes les personnes de l'app
            {
                foreach (var u in zoneck_server.GetAllSessions().ToList())
                {
                    if ((u.SessionID != session.SessionID)
                        &&
                            usersConnected.FirstOrDefault(x => x.userID == u.SessionID).userApp == message.Substring( // envois uniquement au gens de l'app
                        0,
                        message.IndexOf("]") + 1))

                    session.Send(usersConnected.FirstOrDefault(x => x.userID == u.SessionID).lastMessage + " %last_message%");
                }

                return;
            }


            foreach (var u in zoneck_server.GetAllSessions().ToList())
            {
                if ((u.SessionID != session.SessionID)
                    &&
                        usersConnected.FirstOrDefault(x => x.userID == u.SessionID).userApp == message.Substring( // envois uniquement au gens de l'app
                    0,
                    message.IndexOf("]") + 1))

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