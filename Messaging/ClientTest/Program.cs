using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chepo.MessagingClient;
using System.Net;

namespace ClientTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Client client = new Client(IPAddress.Parse("127.0.0.1"), 1337, false);
            client.ClientGetrennt += (g, f) =>
                {
                    Console.WriteLine("Client getrennt");
                };
            client.ClientVerbunden += (v, c) =>
                {
                    Console.WriteLine("Client verbunden.");
                };

            client.Verbinden();

            client.SendeAnStream(@"Ein Test ist ein Versuch, mit dem größere Sicherheit darüber gewonnen werden soll, ob ein technischer Apparat oder ein Vorgang innerhalb der geplanten Rahmenbedingungen funktioniert.");

            while (client.Verbunden)
            {
                
            }

            Console.ReadLine();
        }
    }
}
