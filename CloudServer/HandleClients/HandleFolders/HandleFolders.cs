using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudServer.HandleClients
{
    public class HandleFolders
    {
        public static void Create(TcpClient clientSocket, string data)
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
                                            if (!Directory.Exists(ClensePath.CleanPath(AuthToken, new DirectoryInfo(SyncPath).Name, "")))
                                                Directory.CreateDirectory(ClensePath.CleanPath(AuthToken, new DirectoryInfo(SyncPath).Name, ""));

                                            string FullPath = ClensePath.CleanPath(AuthToken, new DirectoryInfo(SyncPath).Name, RelevantPath);

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
    }
}
