using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CloudServer
{
    public class Connection
    {
        public int clientNumber;
        public TcpClient clientSocket;
        public string IPAddress;

        //Client Vars
        public Thread thread;
        public bool disconnected;
        public NetworkStream networkStream;
        public DateTime timeout = DateTime.Now;
        public HandleClient clientHandle;

        public Connection(int clientNumber, TcpClient clientSocket)
        {
            this.clientNumber = clientNumber;
            this.clientSocket = clientSocket;
            this.IPAddress = ((IPEndPoint)clientSocket.Client.RemoteEndPoint).Address.ToString();
        }
    }
}
