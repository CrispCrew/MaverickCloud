﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CloudClient
{
    public class Client
    {
        /// <summary>
        /// The Server ClientSocket
        /// </summary>
        public static TcpClient clientSocket = new TcpClient();

        /// <summary>
        /// The Network Stream
        /// </summary>
        public static NetworkStream serverStream = null;

        public Client()
        {
            //Log Init
            Console.WriteLine("Logging Init");

            //Establish Server Connection
            Console.WriteLine("Client Started");

            Connect();
        }

        private void Connect()
        {
            Console.WriteLine("Connecting...");

            clientSocket.NoDelay = true;

            clientSocket.Connect("127.0.0.1", 6969);

            Console.WriteLine("Client Socket Program - Server Connected ...");
        }

        /// <summary>
        /// Contacts server for Version Number
        /// </summary>
        /// <returns>Version</returns>
        public string Version()
        {
            if (!clientSocket.Connected)
                Connect();

            CleanStream();

            //Sockets Connection
            //Debug - Log Times
            Stopwatch timer = new Stopwatch();
            timer.Start();

            string Response = API.SendAPIRequest(clientSocket, "Request=Version");

            Console.WriteLine(timer.Elapsed.TotalMilliseconds + "ms");

            timer.Reset();

            return Response;
        }

        /// <summary>
        /// Contacts server for Login Check
        /// </summary>
        /// <param name="Username">Username</param>
        /// <param name="Password">Password</param>
        /// <param name="HWID">Hardware ID</param>
        /// <param name="Token">Users Token</param>
        /// <returns></returns>
        public string Login(string Username, string Password, out string Token)
        {
            string Success;

            if (!clientSocket.Connected)
                Connect();

            CleanStream();

            Token = "";

            if (!Prepare.PrepareString(Username) || !Prepare.PrepareString(Password))
            {
                if (!Prepare.PrepareString(Username))
                    Console.WriteLine("Prepare Failed: Username=" + Username);
                else if (!Prepare.PrepareString(Password))
                    Console.WriteLine("Prepare Failed: Password=" + Password);

                return "Empty Credentials";
            }

            //Sockets Connection
            //Debug - Log Times
            Stopwatch timer = new Stopwatch();
            timer.Start();

            string Response = API.SendAPIRequest(clientSocket, "Request=Login&Username=" + Username + "&Password=" + Password);

            if (Response.Split('-')[0] == "Login Found")
            {
                Token = Response.Split('-')[1];

                Console.WriteLine("Login Found: " + Username + " -> " + Password + " -> " + Token);

                Success = "Login Found";
            }
            else
            {
                Console.WriteLine("Error: Login not Found -> " + Response);

                Success = Response;
            }

            Console.WriteLine(timer.Elapsed.TotalMilliseconds + "ms");

            timer.Reset();

            return Success;
        }

        /// <summary>
        /// Contacts server for a download - Cheat / Update
        /// </summary>
        /// <param name="Token">User Token</param>
        /// <param name="File">File to Download</param>
        /// <param name="productid">Product ID</param>
        /// <returns></returns>
        public bool Upload(string Token, string SyncFolder, string FullPath, string Name)
        {
            if (!clientSocket.Connected)
                Connect();

            CleanStream();

            if (!Prepare.PrepareString(Token))
            {
                if (!Prepare.PrepareString(Token))
                    Console.WriteLine("Prepare Failed: Token=" + Token);

                return false;
            }

            //Sockets Connection
            //Debug - Log Times
            Stopwatch timer = new Stopwatch();
            timer.Start();

            serverStream = clientSocket.GetStream();
            byte[] outStream = Encoding.ASCII.GetBytes("Request=Upload&Token=" + Token + "&Folder=" + "" + "&Path=" + FullPath + "&Name=" + Name);
            byte[] outSize = BitConverter.GetBytes(outStream.Length);

            Console.WriteLine("Raw Data: " + BitConverter.ToInt32(outSize, 0) + " -> " + Encoding.ASCII.GetString(outStream));

            //Write Bytes
            serverStream.Write(outSize, 0, outSize.Length);
            serverStream.Write(outStream, 0, outStream.Length);
            serverStream.Flush();

            //Wait for Response - TODO: Add Recieve Byte outSize

            using (FileStream fileStream = File.OpenRead(FullPath))
            {
                long expectedsize = File.ReadAllBytes(FullPath).LongLength;

                Console.WriteLine("File: " + FullPath + " Size: " + expectedsize);

                outStream = Encoding.ASCII.GetBytes("Size=" + expectedsize);
                outSize = BitConverter.GetBytes(outStream.Length);

                Console.WriteLine("Raw Data: " + BitConverter.ToInt32(outSize, 0) + " -> " + Encoding.ASCII.GetString(outStream));

                //Write Bytes
                serverStream.Write(outSize, 0, outSize.Length);
                serverStream.Write(outStream, 0, outStream.Length);
                serverStream.Flush();

                int totalBytes = 0;
                int bytesRead = 0;
                while (true)
                {
                    byte[] buffer = new byte[13106]; //65536 Bytes * 2 = 1310720 Bytes/ps || 10 Mbps

                    if ((totalBytes + buffer.Length) <= expectedsize) //sent bytes + sending bytes smaller than or equal to Size of File
                    {
                        if ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            serverStream.Write(buffer, 0, bytesRead);

                            totalBytes += bytesRead;

                            Console.Write("\rNetwork Size: {0} Bytes Read: {1}", buffer.Length, totalBytes);
                        }
                        else
                        {
                            Console.WriteLine("FileStream == 0");

                            break;
                        }
                    }
                    else
                    {
                        if ((bytesRead = fileStream.Read(buffer, 0, (int)expectedsize - totalBytes)) > 0)
                        {
                            serverStream.Write(buffer, 0, bytesRead);

                            totalBytes += bytesRead;

                            Console.WriteLine("\rNetwork Size: {0} Bytes Read: {1}", buffer.Length, totalBytes);
                        }
                        else
                        {
                            Console.WriteLine("FileStream == 0");

                            break;
                        }
                    }

                    Thread.Sleep(10); //10 mbps
                }

                serverStream.Flush();
            }

            Console.WriteLine(timer.Elapsed.TotalMilliseconds + "ms");

            timer.Reset();

            return true;
        }

        public bool Download(string Token, string SyncFolder, string FullPath, string Name)
        {
            if (!clientSocket.Connected)
                Connect();

            CleanStream();

            if (!Prepare.PrepareString(Token))
            {
                if (!Prepare.PrepareString(Token))
                    Console.WriteLine("Prepare Failed: Token=" + Token);

                return false;
            }

            Stopwatch timer = new Stopwatch();
            timer.Start();

            serverStream = clientSocket.GetStream();
            byte[] outStream = Encoding.ASCII.GetBytes("Request=Upload&Token=" + Token + "&Folder=" + "" + "&Path=" + FullPath + "&Name=" + Name);
            byte[] outSize = BitConverter.GetBytes(outStream.Length);

            Console.WriteLine("Raw Data: " + BitConverter.ToInt32(outSize, 0) + " -> " + Encoding.ASCII.GetString(outStream));

            //Write Bytes
            serverStream.Write(outSize, 0, outSize.Length);
            serverStream.Write(outStream, 0, outStream.Length);
            serverStream.Flush();

            byte[] size = new byte[4];

            serverStream.Read(size, 0, size.Length);

            byte[] bytesFrom = new byte[BitConverter.ToInt32(size, 0)];

            Console.WriteLine("ExpectedSize: " + BitConverter.ToInt32(size, 0) + " bytesFrom Length: " + bytesFrom.Length);

            serverStream.Read(bytesFrom, 0, bytesFrom.Length);

            string returndata = Encoding.ASCII.GetString(bytesFrom); //Out of memory????

            Console.WriteLine("Data from Server: " + returndata);

            if (Request.Contains("Size", returndata))
            {
                long ExpectedSize = Convert.ToInt64(Request.Get("Size", returndata));

                //Chunked Data Sizes
                byte[] buffer = new byte[4096];

                using (FileStream fileStream = new FileStream(FullPath, FileMode.OpenOrCreate))
                {
                    long bytesReadTotal = 0;
                    int bytesRead = 0;
                    while (ExpectedSize > bytesReadTotal && (bytesRead = serverStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fileStream.Write(buffer, 0, bytesRead);
                        bytesReadTotal += bytesRead;

                        Console.WriteLine("\rNetwork Size: {0} Bytes Read: {1}", bytesRead, bytesReadTotal);

                        double pctComplete = ((double)bytesReadTotal / ExpectedSize) * 100;

                        Console.WriteLine("Completed: {0}%", pctComplete);
                    }

                    fileStream.Dispose();
                }

                Console.WriteLine("Download Completed: " + FullPath + " -> Size: " + ExpectedSize);
            }
            else
            {
                Console.WriteLine("Size Parameter was not provided");
            }

            Console.WriteLine(timer.Elapsed.TotalMilliseconds + "ms");

            timer.Reset();

            return true;
        }

        private void CleanStream()
        {
            if (!clientSocket.Connected)
                Connect();

            if (serverStream == null)
                return;

            Console.WriteLine("Cleaning Stream");

            byte[] buffer = new byte[4096];

            while (serverStream.DataAvailable)
            {
                Console.WriteLine("Cleared Data");

                serverStream.Read(buffer, 0, buffer.Length);
            }
        }
    }
}