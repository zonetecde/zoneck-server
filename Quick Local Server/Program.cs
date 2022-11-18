using sck_server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quick_Local_Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            START:
            Console.WriteLine("Entrez une adresse Ip ou laisser vide pour utiliser : 127.0.0.1 (fonctionnera sur cette ordinateur)");
            string output = Console.ReadLine();
            string ip = String.IsNullOrWhiteSpace(output) ? "127.0.0.1" : output;

            Console.WriteLine("Entrez un port ou laisser vide pour utiliser : 30000");
            output = Console.ReadLine();
            string port = String.IsNullOrWhiteSpace(output) ? "30000" : output;

            try
            {
                ZoneckServer serveur = new ZoneckServer(ip, Convert.ToInt32(port));
            }
            catch
            {
                Console.WriteLine("Information invalide.");
                goto START;
            }
        }
    }
}
