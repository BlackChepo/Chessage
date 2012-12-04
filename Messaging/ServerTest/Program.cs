using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chepo.MessagingServer;
using System.Timers;

namespace ServerTest
{
    class Program
    {
        static Server server;

        static void Main(string[] args)
        {
            server = new Server(1337, null,false);
            server.ServerStart += (s, d) =>
                {
                    Console.WriteLine("SERVER START");
                };
            server.ServerStop += (e, r) =>
                {
                    Console.WriteLine("SERVER STOP");
                };

            server.Start();

            Timer timer = new Timer(1000);
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            timer.Start();
            

            while (server.Working)
            {
                
            }

            Console.ReadLine();
        }

        static void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            List<Client> clients = new List<Client>();
            clients.Add(server.ClientPool.LastOrDefault());
            server.Sende("Broadcast !!!", clients);
        }
    }
}
