using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Timers;

namespace Chepo.MessagingServer
{
    public class Server
    {
        #region Variablen
        internal TcpListener listener = null;
        public event EventHandler ServerStart;
        public event EventHandler ServerStop;        
        #endregion
        #region Eigenschaften
        /// <summary>
        /// Gibt den Zustand des Servers an.
        /// </summary>
        [DefaultValue(false)]
        public bool Working { get; internal set; }
        public IPAddress IP { get; private set; }
        public int Port { get; private set; }
        public List<Client> ClientPool { get; internal set; }
        #endregion
        #region Konstruktor
        /// <summary>
        /// Kontstruktor.
        /// Startet den Server
        /// </summary>        
        /// <param name="port">Port den dieser Server nutzen soll.</param>
        /// <param name="ipadresse">[Optinal] (Any) IP Adresse die dieser Server nutzen soll.</param>
        /// <param name="start">[Optinal] (true) Gibt an, ob der Server sofort gestartet werden soll.</param>
        public Server(int port, IPAddress ipadresse = null, bool start = true)
        {
            IPAddress ip = ipadresse;

            if (ipadresse == null)
                ip = IPAddress.Any;

            if (!(port > 0 && port <= 65535))
                throw new ArgumentOutOfRangeException("Kein gültiger Port.");            

            IP = ip;
            Port = port;

            if (start)
                Start();
        }
        #endregion
        #region Öffentliche Funktionen
        /// <summary>
        /// Startet den Server.
        /// Event: ServerStart       
        /// </summary>
        /// <example>if(!server.Working) server.Start();</example>
        public void Start()
        {
            if (Working)
                return;

            if (listener == null)
            {
                if (IP == null || !(Port > 0 && Port <= 65535))
                    throw new ArgumentOutOfRangeException("Kein gültiger Port.");

                listener = new TcpListener(IP, Port);
            }

            Working = true;

            if (ServerStart != null)
                ServerStart(this, null);

            HalteNachClientsAusschau();
        }

        /// <summary>
        /// Stoppt den Server.
        /// Event: ServerStop
        /// </summary>
        /// <example>if(server.Working) server.Stop();</example>
        public void Stop()
        {
            if (!Working)
                return;

            Working = false;

            if (ServerStop != null)
                ServerStop(this, null);
        }

        /// <summary>
        /// Sendet eine Nachricht.
        /// </summary>
        /// <param name="clients">[Optional] (null) Sendet eine Nachricht an den bestimmte Clients oder an alle.</param>
        public void Sende(string message,IEnumerable<Client> clients = null)
        {
            IEnumerable<Client> empfänger = null;

            if (clients == null)
                empfänger = ClientPool;
            else
                empfänger = clients;

            if (empfänger == null || !empfänger.Any())
                return;

            foreach (var client in empfänger)
            {
                if (client != null)
                    client.SendeAnStream(message);
            }
        }
        #endregion
        #region Private Funktionen
        private void HalteNachClientsAusschau()
        {
            if (ClientPool == null)
                ClientPool = new List<Client>();

            Task.Factory.StartNew(() =>
                {
                    try
                    {
                        listener.Start();

                        while (this.Working)
                        {
                            try
                            {
                                TcpClient client = listener.AcceptTcpClient();

                                try
                                {
                                    Client cl = new Client(client);
                                    cl.StreamGetrennt += (a, b) =>
                                        {
                                            Console.WriteLine("Client Getrennt");

                                            if (a is Client)
                                                ClientPool.Remove(a as Client);
                                        };
                                    ClientPool.Add(cl);
                                }
                                catch (NullReferenceException ex)
                                {
                                    ex.verarbeite();
                                    continue;
                                }        
                            }
                            catch (SocketException ex)
                            {
                                ex.verarbeite();
                                if (Working)
                                    throw;
                            }                    
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.verarbeite();
                        throw;
                    }
                    finally
                    {
                        listener.Stop();
                        Stop();
                    }
                });
        }
        #endregion
    }
}
