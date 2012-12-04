using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Threading.Tasks;

namespace Chepo.MessagingServer
{
    public class Client
    {
        public event EventHandler StreamGetrennt;
        public event EventHandler NeuerStream;
        
        public bool Verbunden 
        { 
            get
            {
                if (TcpClient == null)
                    return false;

                return TcpClient.Connected;
            }
        }

        public Client(TcpClient client)
        {
            if (client == null)
                throw new NullReferenceException("Client ist null");

            TcpClient = client;

            HöreStreamAb();
        }

        private TcpClient _TcpClient = null;
        public TcpClient TcpClient
        {
            get
            {
                if (_TcpClient == null)
                    throw new NullReferenceException("Client ist null");

                return _TcpClient;
            }
            internal set
            {
                _TcpClient = value;
                Stream = value.GetStream();
            }
        }

        private NetworkStream _Stream = null;
        public NetworkStream Stream
        {
            get
            {
                if (_Stream == null)
                {
                    if (TcpClient == null)
                        throw new NullReferenceException("Client ist Null bei Stream.");

                    _Stream = TcpClient.GetStream();
                }

                return _Stream;
            }
            private set
            {
                _Stream = value;
                if (NeuerStream != null)
                    NeuerStream(this, null);
                Console.WriteLine("Neuer Stream");
            }
        }

        public void HöreStreamAb()
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
                        
                        // Incoming message may be larger than the buffer size. 
                        do
                        {
                            try 
	                        {	        
		                        numberOfBytesRead = Stream.Read(readBuffer, 0, readBuffer.Length);
	                        }
	                        catch (IOException)
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

        public void SendeAnStream(string message)
        {
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
                    catch (IOException ex)
                    {
                        ex.verarbeite();
                        StreamTrennen();
                    }
                });
        }

        private void StreamTrennen()
        {            
            if (StreamGetrennt != null)
                StreamGetrennt(this, null);
        }
    }
}
