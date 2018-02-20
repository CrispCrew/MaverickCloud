using CloudServer.HandleClients;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CloudServer
{
    public class HandleClient
    {
        /// <summary>
        /// Starts the Socket Client Async
        /// </summary>
        /// <param name="inClientSocket">fhfshfshfshfs</param>
        /// <param name="clineNo">fhfshfshfshfs</param>
        public void startClient(Connection connection)
        {
            connection.clientHandle = this;

            connection.thread = new Thread(() => doChat(connection));
            connection.thread.Start();
        }

        private void doChat(Connection connection)
        {
            int requestCount = 0;

            while (true)
            {
                byte[] size;
                byte[] bytesFrom;
                string dataFromClient = null;
                string serverResponse = null;

                Stopwatch timer = new Stopwatch();
                timer.Start();

                try
                {
                    requestCount++;

                    if (connection.clientSocket == null || !connection.clientSocket.Connected || connection.disconnected)
                    {
                        try
                        {
                            connection.clientSocket.Close();
                        }
                        catch
                        {

                        }

                        break;
                    }

                    string IPAddress = ((IPEndPoint)connection.clientSocket.Client.RemoteEndPoint).Address.ToString();

                    connection.networkStream = connection.clientSocket.GetStream();

                    size = new byte[4];

                    if (connection.networkStream.Read(size, 0, size.Length) == 0)
                    {
                        Console.WriteLine("Request == 0");

                        Thread.Sleep(1000);

                        continue;
                    }
                    else if (BitConverter.ToInt32(size, 0) == 0)
                    {
                        serverResponse = "Too Small of a Request [" + BitConverter.ToInt32(size, 0) + "]";
                    }
                    else if (BitConverter.ToInt32(size, 0) > 1048576)
                    {
                        serverResponse = "Too Large of a Request [" + BitConverter.ToInt32(size, 0) + "]";
                    }
                    else
                    {
                        bytesFrom = new byte[BitConverter.ToInt32(size, 0)];

                        Console.WriteLine("ExpectedSize: " + BitConverter.ToInt32(size, 0) + " bytesFrom Length: " + bytesFrom.Length);

                        connection.networkStream.Read(bytesFrom, 0, bytesFrom.Length);
                        dataFromClient = Encoding.ASCII.GetString(bytesFrom);

                        Console.WriteLine("Raw Data: " + dataFromClient);

                        serverResponse = HandleCommand.ParseCommand(connection.clientSocket, dataFromClient);
                    }

                    //Byte Streams
                    byte[] outStream = Encoding.ASCII.GetBytes(serverResponse);

                    byte[] outSize = BitConverter.GetBytes(outStream.Length);

                    //Network Streams
                    connection.networkStream.Write(outSize, 0, outSize.Length);
                    connection.networkStream.Write(outStream, 0, outStream.Length);
                    connection.networkStream.Flush();

                    Console.WriteLine(">> Size=" + BitConverter.ToInt32(outSize, 0) + " Response: " + serverResponse);

                    connection.timeout = DateTime.Now;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(">> " + ex.ToString());

                    connection.disconnected = true;

                    break;
                }

                Console.WriteLine(timer.Elapsed.TotalMilliseconds + "ms");

                timer.Reset();
            }
        }
    }
}
