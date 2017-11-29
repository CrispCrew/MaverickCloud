using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CloudServer.HandleClients
{
    public class HandleFiles
    {
        /// <summary>
        /// Calls UploadFile ( Upload to Client )
        /// </summary>
        /// <param name="clientSocket"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string UploadFile(TcpClient clientSocket, string data)
        {
            string Token;
            string Folder;
            string Path;
            string Name;
            string ServerResponse = null;

            if (Request.Contains("Folder", data))
            {
                Folder = Request.Get("Folder", data);

                if (Request.Contains("Token", data))
                {
                    Token = Request.Get("Token", data);

                    Token token = Tokens.GetTokenByToken(Token);

                    if (Request.Contains("Path", data))
                    {
                        Path = Request.Get("Path", data);

                        if (Request.Contains("Name", data))
                        {
                            Name = Request.Get("Name", data);

                            if (UploadFile(clientSocket.GetStream(), Environment.CurrentDirectory + "\\Users\\" + token.Username + "\\" + Folder + "\\" + Name, Name))
                            {
                                ServerResponse = "File Upload Completed";
                            }
                            else
                            {
                                ServerResponse = "File Upload Failed";
                            }
                        }
                        else
                        {
                            ServerResponse = "Name Parameter was not provided";
                        }
                    }
                    else
                    {
                        ServerResponse = "File Parameter was not provided";
                    }
                }
                else
                {
                    ServerResponse = "Token Parameter was not provided";
                }
            }
            else
            {
                ServerResponse = "Folder Parameter was not provided";
            }

            return ServerResponse;
        }

        /// <summary>
        /// Calls DownloadFile ( Download from Client )
        /// </summary>
        /// <param name="clientSocket"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string DownloadFile(TcpClient clientSocket, string data)
        {
            string Token;
            string Folder;
            string Path;
            string Name;
            string ServerResponse = null;

            if (Request.Contains("Token", data))
            {
                Token = Request.Get("Token", data);

                Token token = Tokens.GetTokenByToken(Token);

                if (Request.Contains("Folder", data))
                {
                    Folder = Request.Get("Folder", data);

                    if (Request.Contains("Path", data))
                    {
                        Path = Request.Get("Path", data);

                        if (Request.Contains("Name", data))
                        {
                            Name = Request.Get("Name", data);

                            if (DownloadFile(clientSocket.GetStream(), Environment.CurrentDirectory + "\\Users\\" + token.Username + "\\" + Folder + "\\" + Name, Name))
                            {
                                ServerResponse = "File Download Completed";
                            }
                            else
                            {
                                ServerResponse = "File Download Failed";
                            }
                        }
                        else
                        {
                            ServerResponse = "Name Parameter was not provided";
                        }
                    }
                    else
                    {
                        ServerResponse = "File Parameter was not provided";
                    }
                }
                else
                {
                    ServerResponse = "Folder Parameter was not provided";
                }
            }
            else
            {
                ServerResponse = "Token Parameter was not provided";
            }

            return ServerResponse;
        }

        /// <summary>
        /// Upload to Client
        /// </summary>
        /// <param name="serverStream"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool UploadFile(NetworkStream serverStream, string path, string name)
        {
            string FullPath = Path.GetDirectoryName(path) + "\\" + name;

            using (FileStream fileStream = File.OpenRead(FullPath))
            {
                long expectedsize = File.ReadAllBytes(FullPath).LongLength;

                Console.WriteLine("File: " + FullPath + " Size: " + expectedsize);

                byte[] outStream = Encoding.ASCII.GetBytes("Size=" + expectedsize);
                byte[] outSize = BitConverter.GetBytes(outStream.Length);

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

                            Console.WriteLine("\rNetwork Size: {0} Bytes Read: {1}", buffer.Length, totalBytes);
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

            return true;
        }

        /// <summary>
        /// Download from Client
        /// </summary>
        /// <param name="serverStream"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool DownloadFile(NetworkStream serverStream, string path, string name)
        {
            string FullPath = Path.GetDirectoryName(path) + "\\" + name;

            Console.WriteLine("FullPath: " + FullPath);

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

            return false;
        }
    }
}
