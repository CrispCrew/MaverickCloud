using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
        public static string Files(TcpClient clientSocket, string data)
        {
            Token AuthToken;
            string Token;
            string ServerResponse = null;

            if (Request.Contains("Token", data))
            {
                Token = Request.Get("Token", data);

                //Get Token by Token -> Get Token by IP Address -> Compare Returned Tokens
                if (Tokens.GetTokenByToken(Token) != null && Tokens.GetToken(((IPEndPoint)clientSocket.Client.RemoteEndPoint).Address.ToString()) != null && Tokens.GetTokenByToken(Token) == Tokens.GetToken(((IPEndPoint)clientSocket.Client.RemoteEndPoint).Address.ToString()))
                {
                    AuthToken = Tokens.GetTokenByToken(Token);

                    //Root of the Sync Path
                    foreach (string file in GetFiles(ClensePath.CleanPath(AuthToken)))
                    {
                        //SyncPath = Folder Name of the Sync Folder

                        //C:\\Server\\CrispyCheats\\MaverickCloud1\\File.txt -> Replace("C:\\Server\\CrispyCheats") = \\MaverickCloud1\\File.txt


                        string SyncPath = file.Replace(ClensePath.CleanPath(AuthToken), "").Replace(new FileInfo(file).Name, "").Replace(@"\", "");

                        Console.WriteLine("Clean Path: " + ClensePath.CleanPath(AuthToken, SyncPath) + " File Path: " + file);

                        string RelevantPath = file.Replace(ClensePath.CleanPath(AuthToken, SyncPath), "").Replace(new FileInfo(file).Name, "");


                        string FileName = new FileInfo(file).Name;

                        Console.WriteLine("File Info: SyncPath={0}, RelevantPath={1}, FileName={2}", SyncPath, RelevantPath, FileName);
                    }

                    ServerResponse = "Files Retrieved";
                }
                else
                {
                    ServerResponse = "Authentication Token not found";
                }
            }
            else
            {
                ServerResponse = "Token Parameter was not provided";
            }

            return ServerResponse;
        }

        /// <summary>
        /// Calls UploadFile ( Upload to Client )
        /// </summary>
        /// <param name="clientSocket"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string UploadFile(TcpClient clientSocket, string data)
        {
            Token AuthToken;
            string Token;
            string SyncPath;
            string RelevantPath;
            string FileName;
            string ServerResponse = null;

            if (Request.Contains("Token", data))
            {
                Token = Request.Get("Token", data);

                //Get Token by Token -> Get Token by IP Address -> Compare Returned Tokens
                if (Tokens.GetTokenByToken(Token) != null && Tokens.GetToken(((IPEndPoint)clientSocket.Client.RemoteEndPoint).Address.ToString()) != null && Tokens.GetTokenByToken(Token) == Tokens.GetToken(((IPEndPoint)clientSocket.Client.RemoteEndPoint).Address.ToString()))
                {
                    AuthToken = Tokens.GetTokenByToken(Token);

                    if (Request.Contains("SyncPath", data))
                    {
                        SyncPath = Request.Get("SyncPath", data);

                        if (Request.Contains("RelevantPath", data))
                        {
                            RelevantPath = Request.Get("RelevantPath", data);
 
                            if (Request.Contains("FileName", data))
                            {
                                FileName = Request.Get("FileName", data);

                                string FullPath = ClensePath.CleanPath(AuthToken, SyncPath, RelevantPath);

                                if (UploadFile(clientSocket.GetStream(), FullPath, FileName, new FileInfo(FullPath + FileName).Length))
                                {
                                    ServerResponse = "Created=" + File.GetCreationTimeUtc(FullPath + FileName).ToBinary() + "&LastModified=" + File.GetLastWriteTimeUtc(FullPath + FileName).ToBinary() + "&Status=File Upload Completed";
                                }
                                else
                                {
                                    ServerResponse = "File Upload Failed";
                                }
                            }
                            else
                            {
                                ServerResponse = "FileName Parameter was not provided";
                            }
                        }
                        else
                        {
                            ServerResponse = "RelevantPath Parameter was not provided";
                        }
                    }
                    else
                    {
                        ServerResponse = "SyncPath Parameter was not provided";
                    }
                }
                else
                {
                    ServerResponse = "Authentication Token not found";
                }
            }
            else
            {
                ServerResponse = "Token Parameter was not provided";
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
            Token AuthToken;
            string Token, SyncPath, RelevantPath, FileName, Created, LastModified, Size;
            string ServerResponse = null;

            if (Request.Contains("Token", data))
            {
                Token = Request.Get("Token", data);

                //Get Token by Token -> Get Token by IP Address -> Compare Returned Tokens
                if (Tokens.GetTokenByToken(Token) != null && Tokens.GetToken(((IPEndPoint)clientSocket.Client.RemoteEndPoint).Address.ToString()) != null && Tokens.GetTokenByToken(Token) == Tokens.GetToken(((IPEndPoint)clientSocket.Client.RemoteEndPoint).Address.ToString()))
                {
                    AuthToken = Tokens.GetTokenByToken(Token);

                    if (Request.Contains("SyncPath", data))
                    {
                        SyncPath = Request.Get("SyncPath", data);

                        if (Request.Contains("RelevantPath", data))
                        {
                            RelevantPath = Request.Get("RelevantPath", data);

                            if (Request.Contains("FileName", data))
                            {
                                FileName = Request.Get("FileName", data);

                                if (Request.Contains("Created", data))
                                {
                                    Created = Request.Get("Created", data);

                                    if (Request.Contains("LastModified", data))
                                    {
                                        LastModified = Request.Get("LastModified", data);

                                        if (Request.Contains("Size", data))
                                        {
                                            Size = Request.Get("Size", data);

                                            //Server Files -> User Folder -> SyncPath Folder -> Files
                                            if (!Directory.Exists(ClensePath.CleanPath(AuthToken, SyncPath, "")))
                                                Directory.CreateDirectory(ClensePath.CleanPath(AuthToken, SyncPath, ""));

                                            string FullPath = ClensePath.CleanPath(AuthToken, SyncPath, RelevantPath);

                                            if (File.Exists(FullPath + FileName))
                                                try { File.Delete(FullPath + FileName); } catch { Console.WriteLine("File Deletion Failed!"); }

                                            if (DownloadFile(clientSocket.GetStream(), FullPath, FileName, Convert.ToInt64(Size)))
                                            {
                                                File.SetCreationTimeUtc(FullPath + FileName, DateTime.FromBinary(Convert.ToInt64(Created)));

                                                File.SetLastWriteTimeUtc(FullPath + FileName, DateTime.FromBinary(Convert.ToInt64(LastModified)));

                                                ServerResponse = "File Download Completed";
                                            }
                                            else
                                            {
                                                ServerResponse = "File Download Failed";
                                            }
                                        }
                                        else
                                        {
                                            ServerResponse = "Size Parameter not found";
                                        }
                                    }
                                    else
                                    {
                                        ServerResponse = "LastModified Parameter not found";
                                    }
                                }
                                else
                                {
                                    ServerResponse = "Created Parameter not found";
                                }
                            }
                            else
                            {
                                ServerResponse = "FileName Parameter was not provided";
                            }
                        }
                        else
                        {
                            ServerResponse = "RelevantPath Parameter was not provided";
                        }
                    }
                    else
                    {
                        ServerResponse = "SyncPath Parameter was not provided";
                    }
                }
                else
                {
                    ServerResponse = "Authentication Token not found";
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
        public static bool UploadFile(NetworkStream serverStream, string path, string name, long size)
        {
            string FullPath = path + "\\" + name;

            using (FileStream fileStream = File.OpenRead(FullPath))
            {
                API.SendAPIRequest(serverStream, "Size=" + size, false);

                int totalBytes = 0;
                int bytesRead = 0;
                while (true)
                {
                    byte[] buffer = new byte[13106]; //65536 Bytes * 2 = 1310720 Bytes/ps || 10 Mbps

                    if ((totalBytes + buffer.Length) <= size) //sent bytes + sending bytes smaller than or equal to Size of File
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
                        if ((bytesRead = fileStream.Read(buffer, 0, (int)size - totalBytes)) > 0)
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

                    Thread.Sleep(1); //10ms = 10 mbps | 1ms = 100mbps
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
        public static bool DownloadFile(NetworkStream serverStream, string path, string name, long size)
        {
            string FullPath = path + "\\" + name;

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            Console.WriteLine("FullPath: " + FullPath);

            //Chunked Data Sizes
            byte[] buffer = new byte[4096];

            using (FileStream fileStream = new FileStream(FullPath, FileMode.OpenOrCreate))
            {
                long bytesReadTotal = 0;
                int bytesRead = 0;
                while (size > bytesReadTotal && (bytesRead = serverStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    fileStream.Write(buffer, 0, bytesRead);
                    bytesReadTotal += bytesRead;

                    Console.WriteLine("\rNetwork Size: {0} Bytes Read: {1}", bytesRead, bytesReadTotal);

                    double pctComplete = ((double)bytesReadTotal / size) * 100;

                    Console.WriteLine("Completed: {0}%", pctComplete);
                }

                fileStream.Dispose();
            }

            Console.WriteLine("Download Completed: " + FullPath + " -> Size: " + size);

            return true;
        }

        private static List<string> GetFiles(string folder)
        {
            List<String> files = new List<String>();

            try
            {
                foreach (string f in Directory.GetFiles(folder))
                {
                    files.Add(f);
                }
                foreach (string d in Directory.GetDirectories(folder))
                {
                    files.AddRange(GetFiles(d));
                }
            }
            catch
            {

            }

            return files;
        }
    }
}
