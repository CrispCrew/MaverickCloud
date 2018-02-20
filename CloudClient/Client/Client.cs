using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

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

            clientSocket.Connect("158.69.255.77", 6060);

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

        public bool APICheck(string Token)
        {
            if (!clientSocket.Connected)
                Connect();

            CleanStream();

            //Sockets Connection
            //Debug - Log Times
            Stopwatch timer = new Stopwatch();
            timer.Start();

            //Send the Cheat Type at Launch
            string Response = API.SendAPIRequest(clientSocket, "Request=APICheck&Token=" + Token);

            Console.WriteLine("Request: " + "LoginCount" + " -> " + "Response: " + Response);

            Console.Write(timer.Elapsed.TotalMilliseconds + "ms");

            timer.Reset();

            if (Response != "Authenticated")
            {
                return false;
            }

            return true;
        }

        public string Files(string Token)
        {
            if (!clientSocket.Connected)
                Connect();

            CleanStream();

            //Sockets Connection
            //Debug - Log Times
            Stopwatch timer = new Stopwatch();
            timer.Start();

            //Send the Cheat Type at Launch
            string Response = API.SendAPIRequest(clientSocket, "Request=Files&Token=" + Token);

            Console.WriteLine("Request: " + "LoginCount" + " -> " + "Response: " + Response);

            Console.Write(timer.Elapsed.TotalMilliseconds + "ms");

            timer.Reset();

            if (Response == "Files Retrieved")
            {
                return "Files Retrieved";
            }

            return "Files Retrieval Failed";
        }

        /// <summary>
        /// Contacts server for a download - Cheat / Update
        /// </summary>
        /// <param name="Token">User Token</param>
        /// <param name="File">File to Download</param>
        /// <param name="productid">Product ID</param>
        /// <returns></returns>
        public bool Upload(string Token, string SyncPath, string RelevantPath, string FileName)
        {
            string FullPath = SyncPath + "\\" + RelevantPath + "\\" + FileName;
            long Created = File.GetCreationTimeUtc(FullPath).ToBinary();
            long LastModified = File.GetLastWriteTimeUtc(FullPath).ToBinary();
            long Size = new FileInfo(FullPath).Length;

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
            byte[] outStream = Encoding.ASCII.GetBytes("Request=Upload&Token=" + Token + "&SyncPath=" + SyncPath + "&RelevantPath=" + RelevantPath + "&FileName=" + FileName + "&Created=" + Created + "&LastModified=" + LastModified + "&Size=" + Size);
            byte[] outSize = BitConverter.GetBytes(outStream.Length);

            Console.WriteLine("Raw Data: " + BitConverter.ToInt32(outSize, 0) + " -> " + Encoding.ASCII.GetString(outStream));

            //Write Bytes
            serverStream.Write(outSize, 0, outSize.Length);
            serverStream.Write(outStream, 0, outStream.Length);
            serverStream.Flush();

            //Wait for Response - TODO: Add Recieve Byte outSize

            using (FileStream fileStream = File.OpenRead(FullPath))
            {
                int totalBytes = 0;
                int bytesRead = 0;
                while (true)
                {
                    byte[] buffer = new byte[13106]; //65536 Bytes * 2 = 1310720 Bytes/ps || 10 Mbps

                    if ((totalBytes + buffer.Length) <= Size) //sent bytes + sending bytes smaller than or equal to Size of File
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
                        if ((bytesRead = fileStream.Read(buffer, 0, (int)Size - totalBytes)) > 0)
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

                    Thread.Sleep(1); //10ms = 10 mbps //1ms = 100mbps
                }

                serverStream.Flush();
            }

            Console.WriteLine(timer.Elapsed.TotalMilliseconds + "ms");

            timer.Reset();

            //Get Response
            byte[] size = new byte[4];

            serverStream.Read(size, 0, size.Length);

            byte[] bytesFrom = new byte[BitConverter.ToInt32(size, 0)];

            Console.WriteLine("ExpectedSize: " + BitConverter.ToInt32(size, 0) + " bytesFrom Length: " + bytesFrom.Length);

            serverStream.Read(bytesFrom, 0, bytesFrom.Length);

            string returndata = Encoding.ASCII.GetString(bytesFrom);

            Console.WriteLine("Data from Server: " + returndata);

            if (returndata == "File Download Completed")
                return true;

            return false;
        }

        public bool Download(string Token, string SyncPath, string RelevantPath, string FileName)
        {
            string FullPath = SyncPath + "\\" + RelevantPath + "\\" + FileName;

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
            byte[] outStream = Encoding.ASCII.GetBytes("Request=Download&Token=" + Token + "&SyncPath=" + SyncPath + "&RelevantPath=" + RelevantPath + "&FileName=" + FileName);
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

            //Get Response
            size = new byte[4];

            serverStream.Read(size, 0, size.Length);

            bytesFrom = new byte[BitConverter.ToInt32(size, 0)];

            Console.WriteLine("ExpectedSize: " + BitConverter.ToInt32(size, 0) + " bytesFrom Length: " + bytesFrom.Length);

            serverStream.Read(bytesFrom, 0, bytesFrom.Length);

            returndata = Encoding.ASCII.GetString(bytesFrom);

            Console.WriteLine("Data from Server: " + returndata);

            string Created;
            string LastModified;
            string Status;

            if (API.Contains("Created", returndata))
            {
                Created = API.Get("Created", returndata);

                if (API.Contains("LastModified", returndata))
                {
                    LastModified = API.Get("LastModified", returndata);

                    if (API.Contains("Status", returndata))
                    {
                        Status = API.Get("Status", returndata);

                        if (Status == "File Upload Completed")
                        {
                            File.SetCreationTimeUtc(FullPath, DateTime.FromBinary(Convert.ToInt64(Created)));

                            File.SetLastWriteTimeUtc(FullPath, DateTime.FromBinary(Convert.ToInt64(LastModified)));

                            Console.WriteLine("File Download Completed");

                            return true;
                        }
                        else
                        {
                            Console.WriteLine("File Download Failed!");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Status Parameter not found");
                    }
                }
                else
                {
                    Console.WriteLine("LastModified Parameter not found");
                }
            }
            else
            {
                Console.WriteLine("Created Parameter not found");
            }

            return false;
        }

        public bool CompareProperties(string Token, string SyncPath, string RelevantPath, string FileName)
        {
            string FullPath = SyncPath + "\\" + RelevantPath + "\\" + FileName;

            long Size = new FileInfo(FullPath).Length;
            DateTime Created = File.GetCreationTimeUtc(FullPath);
            DateTime LastModified = File.GetLastWriteTimeUtc(FullPath);

            string response = API.SendAPIRequest(clientSocket, "Request=CompareProperties&Token=" + Token + "&Size=" + Size + "&Created=" + Created.ToBinary() + "&LastModified=" + LastModified.ToBinary() + "&SyncPath=" + SyncPath + "&RelevantPath=" + RelevantPath + "&FileName=" + FileName);

            Console.WriteLine(response);

            if (response == "Same")
                return true;

            return false;
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
