using CloudServer.HandleClients;
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
    public class Program
    {
        public static List<Connection> TcpClients = new List<Connection>();

        public static void Main(string[] args)
        {
            new Thread(serverThread).Start();

            new Thread(cacheThread).Start();
        }

        private static void serverThread()
        {
            TcpListener serverSocket = new TcpListener(6969);

            TcpClient clientSocket = default(TcpClient); //ClientSocket
            serverSocket.Start();
            Console.WriteLine(" >> " + "Server Started");

            int counter = 0;
            while (true)
            {
                try
                {
                    counter += 1;
                    clientSocket = serverSocket.AcceptTcpClient();

                    string IPAddress = ((IPEndPoint)clientSocket.Client.RemoteEndPoint).Address.ToString();

                    Console.WriteLine("IPAddress: " + IPAddress);

                    clientSocket.NoDelay = true;

                    clientSocket.ReceiveTimeout = (60000 * 5);

                    Console.WriteLine(" >> " + "Client No:" + Convert.ToString(counter) + " started!");

                    Connection connection = new Connection(counter, clientSocket);
                    HandleClient client = new HandleClient();
                    client.startClient(connection);

                    lock (TcpClients)
                    {
                        TcpClients.Add(connection);
                    }

                    Console.WriteLine("Client Connected [" + IPAddress + "]", "Connections");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Client Connection Failed", "DEBUG");
                    Console.WriteLine(ex.ToString(), "DEBUG");
                }
            }

            clientSocket.Close();
            serverSocket.Stop();
            Console.WriteLine(" >> " + "exit");
            Console.ReadLine();
        }

        private static void cacheThread()
        {
            DateTime date = DateTime.Now;

            while (true)
            {
                List<Connection> TcpClientsTemp;

                lock (TcpClients)
                {
                    TcpClientsTemp = new List<Connection>(TcpClients);
                }

                if (TcpClientsTemp.Count > 0)
                {
                    int index = 0;
                    int removed = 0;
                    foreach (Connection client in new List<Connection>(TcpClientsTemp))
                    {
                        if (client == null || client.clientHandle == null || client.clientSocket == null || !client.clientSocket.Connected || client.disconnected || client.timeout.AddMinutes(5) < DateTime.Now)
                        {
                            //Dispose Object???
                            try
                            {
                                if (client.clientSocket != null)
                                {
                                    client.clientSocket.Close();

                                    client.clientSocket = null;
                                }

                                if (client.thread != null)
                                {
                                    client.thread.Abort();

                                    client.thread = null;
                                }

                                if (client.networkStream != null)
                                {
                                    client.networkStream = null;
                                }

                                if (client.clientHandle != null)
                                {
                                    client.clientHandle = null;
                                }

                                if ((index - removed) <= TcpClientsTemp.Count)
                                {
                                    Console.WriteLine("Removing TCPClient Index {0} Calculated Index: {1} List Length: {2}", index, (index - removed), TcpClientsTemp.Count);

                                    Console.WriteLine("Disposing Socket", "TCPCleanUp");

                                    TcpClientsTemp.RemoveAt((index - removed));

                                    removed++;
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Disposing Socket Failed" + " Failed", "ERROR");
                                Console.WriteLine(ex.ToString(), "ERROR");
                            }
                        }

                        index++;
                    }

                    lock (TcpClients)
                    {
                        TcpClients = TcpClientsTemp;
                    }
                }

                List<Token> AuthTokensTemp;

                lock (Tokens.AuthTokens)
                {
                    AuthTokensTemp = new List<Token>(Tokens.AuthTokens);
                }

                if (AuthTokensTemp.Count > 0)
                {
                    int index = 0;
                    int removed = 0;
                    foreach (Token token in new List<Token>(AuthTokensTemp))
                    {
                        //If key is older than 5 minutes / hasnt been touched in 5 mins, delete it
                        if (token.LastRequest.AddMinutes(5) < DateTime.Now)
                        {
                            Console.WriteLine(token.Username + "'s token is too old and is being removed!");

                            if ((index - removed) <= AuthTokensTemp.Count)
                            {
                                Console.WriteLine("Removing Token Index {0} New Index: {1} List Length: {2}", index, (index - removed), AuthTokensTemp.Count);

                                Console.WriteLine("Disposing Token [" + token.AuthToken + "]", "TokenCleanUp");

                                AuthTokensTemp.RemoveAt((index - removed));

                                removed++;
                            }
                        }

                        index++;
                    }

                    lock (Tokens.AuthTokens)
                    {
                        Tokens.AuthTokens = AuthTokensTemp;
                    }
                }

                Thread.Sleep(1000);
            }
        }
    }
}
