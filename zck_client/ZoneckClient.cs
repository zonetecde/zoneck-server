﻿using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using static zck_client.ClassLibrary;

namespace zck_client
{
    public class ZoneckClient
    {
        private Socket SocketClient { get; set; }
        public string MyId { get; set; }
        internal Action<Message> Receive { get; }

        public ZoneckClient(string appName, string ip, int port, Action<Message> Receive)
        {
            // TODO: Utiliser l'appName 

            // Créer une instance du SocketClient
            SocketClient = new Socket(SocketType.Stream, ProtocolType.Tcp);
            IPAddress _ip = IPAddress.Parse(ip);
            IPEndPoint point = new IPEndPoint(_ip, port);

            // Connexion
            SocketClient.Connect(point);

            // Thread pour recevoir les messages du serveur en continue
            Thread thread = new Thread(_Receive);
            thread.IsBackground = true;
            thread.Start();

            this.Receive = Receive;
        }

        /// <summary>
        /// Envois un message à tous ou un utilisateur du serveur
        /// </summary>
        /// <param name="str">Le message à envoyer</param>
        public void Send(string str, string toId = "")
        {
            string msg = JsonConvert.SerializeObject(new Message(MyId, str, MESSAGE_TYPE.MESSAGE, toId)) + "\r\n";

            var buffter = Encoding.UTF8.GetBytes(msg);
            var temp = SocketClient.Send(buffter);
        }

        /// <summary>
        /// Demande le contenue d'un fichier stocké sur le serveur
        /// </summary>
        /// <param name="path">Nom du fichier demandé (ex : folder/file.txt)</param>
        /// <remarks>Le contenue du fichier sera envoyé dans votre fonction MessageReceived. Ce sera un message de type "GET_FILE". Son contenue se trouvera donc dans le msg.Content</remarks>
        public void GetFile(string path)
        {
            string msg = JsonConvert.SerializeObject(new Message(MyId, path, MESSAGE_TYPE.GET_FILE)) + "\r\n";

            var buffter = Encoding.UTF8.GetBytes(msg);
            var temp = SocketClient.Send(buffter);
        }

        /// <summary>
        /// Créé un fichier sur le serveur
        /// </summary>
        /// <param name="path">Nom du fichier a créé (ex : folder/file.txt)</param>
        public void CreateFile(string path)
        {
            string msg = JsonConvert.SerializeObject(new Message(MyId, path, MESSAGE_TYPE.CREATE_FILE)) + "\r\n";

            var buffter = Encoding.UTF8.GetBytes(msg);
            var temp = SocketClient.Send(buffter);
        }

        /// <summary>
        /// Supprime un fichier sur le serveur
        /// </summary>
        /// <param name="path">Chemin du fichier à supprimer</param>
        /// <param name="warnOtherPeople">Prévenur les autres utilisateurs qu'un fichier a été supprimé</param>
        /// <remarks>Si warnOtherPeople est activé, Les personnes connectées au serveur reçevrons une alerte envoyé dans leur fonction MessageReceived. Ce sera un message de type "FILE_DELETED". Le chemin d'accès au fichier se trouvera dans msg.Content</remarks>
        public void DeleteFile(string path, bool warnOtherPeople)
        {
            string msg = JsonConvert.SerializeObject(new Message(MyId, JsonConvert.SerializeObject(new FileMessage(path, warnOtherPeople)), MESSAGE_TYPE.FILE_DELETED)) + "\r\n";

            var buffter = Encoding.UTF8.GetBytes(msg);
            var temp = SocketClient.Send(buffter);
        }

        /// <summary>
        /// Modifie un fichier sur le serveur
        /// </summary>
        /// <param name="path">Le chemin d'accès au fichier</param>
        /// <param name="updatedFileContent">Le nouveau contenue du fichier</param>
        /// <param name="warnOtherPeople">Prévenur les autres utilisateurs qu'un fichier a été modifié</param>
        /// <remarks>Si warnOtherPeople est activé, Les personnes connectées au serveur reçevrons une alerte envoyé dans leur fonction MessageReceived. Ce sera un message de type "FILE_UPDATED". Il faudra désérialiser le msg.Content en type "FileMessage". Le chemin d'accès au fichier se trouvera dans fM.Path, et le nouveau contenu dans fM.Content.</remarks>
        public void UpdateFile(string path, string updatedFileContent, bool warnOtherPeople)
        {
            string msg = JsonConvert.SerializeObject(new Message(MyId, JsonConvert.SerializeObject(new FileMessage(path, warnOtherPeople, updatedFileContent)), MESSAGE_TYPE.FILE_UPDATED)) + "\r\n";

            var buffter = Encoding.UTF8.GetBytes(msg);
            var temp = SocketClient.Send(buffter);
        }

        /// <summary>
        /// Retourne dans un message de type LIST_FILE (dans votre fonction MessageReceived) tous les fichiers du serveur séparé par une virgule.
        /// </summary>
        public void ListFile()
        {
            string msg = JsonConvert.SerializeObject(new Message(MyId, "I wanna receive files", MESSAGE_TYPE.LIST_FILE)) + "\r\n";

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