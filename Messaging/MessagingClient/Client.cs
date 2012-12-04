using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.ComponentModel;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.IO;

namespace Chepo.MessagingClient
{
    public class Client
    {
        #region Variablen
        private TcpClient client = null;
        public event EventHandler ClientGetrennt;
        public event EventHandler ClientVerbunden;
        #endregion
        #region Eigenschaften
        public IPAddress IP { get; internal set; }
        public int Port { get; internal set; }
        public NetworkStream Stream { get; private set; }

        /// <summary>
        /// Gibt den Status des Clients an.
        /// </summary>        
        public bool Verbunden
        {
            get
            {
                if (client == null)
                    return false;

                return client.Connected;
            }
        }
        #endregion
        #region Konstruktor
        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="ipadresse">Ziel IP</param>
        /// <param name="port">Ziel Port</param>
        /// <param name="connect">[Optional] (false) sofort verbinden</param>
        public Client(IPAddress ipadresse, int port, bool connect = true)
        {
            IPAddress ip = ipadresse;

            if (ipadresse == null)
                throw new ArgumentNullException("IP ist null.");

            if (!(port > 0 && port <= 65535))
                throw new ArgumentOutOfRangeException("Kein gültiger Port.");

            IP = ip;
            Port = port;

            if (connect)
                Verbinden();
        }
        #endregion
        #region Öffentliche Funktionen
        public void Verbinden()
        {
            if (client == null)
                client = new TcpClient();

            try
            {
                client.Connect(IP, Port);
                Stream = client.GetStream();

                if (ClientVerbunden != null)
                    ClientVerbunden(this, null);

                HöreStreamAb();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void SendeAnStream(string message)
        {
            if (!Verbunden)
                Verbinden();

            if (Stream == null)
                StreamTrennen();

            Task.Factory.StartNew(() =>
            {
                try
                {
                    ASCIIEncoding encoder = new ASCIIEncoding();
                    byte[] buffer = encoder.GetBytes(message);

                    Stream.Write(buffer, 0, buffer.Length);
                    Stream.Flush();
                }
                catch (IOException)
                {                    
                    StreamTrennen();
                }
            });
        }
        #endregion
        #region Private Funktionen
        private void HöreStreamAb()
        {
            if (Stream == null)
                StreamTrennen();

            Task.Factory.StartNew(() =>
            {
                bool nochVerbunden = true;
                while (nochVerbunden)
                {
                    byte[] readBuffer = new byte[1024];
                    StringBuilder completeMessage = new StringBuilder();
                    int numberOfBytesRead = 0;
                    
                    do
                    {
                        try
                        {
                            numberOfBytesRead = Stream.Read(readBuffer, 0, readBuffer.Length);
                        }
                        catch (IOException ex)
                        {
                            nochVerbunden = false;
                            break;
                            // Keine Verbindung zum Stream.                    
                        }

                        completeMessage.AppendFormat("{0}", Encoding.ASCII.GetString(readBuffer, 0, numberOfBytesRead));
                    }
                    while (Stream.DataAvailable);

                    if (!string.IsNullOrWhiteSpace(completeMessage.ToString()))
                        Console.WriteLine(completeMessage);
                }

                StreamTrennen();
            });
        }

        private void StreamTrennen()
        {
            if (ClientGetrennt != null)
                ClientGetrennt(this, null);

            Stream.Close();            
            client = new TcpClient();
        }
        #endregion
    }
}
