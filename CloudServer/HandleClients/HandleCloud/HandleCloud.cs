using CloudServer.HandleClients.Functions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CloudServer.HandleClients
{
    public class HandleCloud
    {
        #region Rename | Delete
        public static string Rename(TcpClient clientSocket, string data)
        {
            Token AuthToken;
            string Token;
            string SyncPath;
            string RelevantPath;
            string FullPath;
            string FileName;
            string OldFileName;
            string ServerResponse;

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

                            if (Request.Contains("FullPath", data))
                            {
                                FullPath = Request.Get("FullPath", data);

                                if (Request.Contains("FileName", data))
                                {
                                    FileName = Request.Get("FileName", data);

                                    if (Request.Contains("OldFileName", data))
                                    {
                                        OldFileName = Request.Get("OldFileName", data);

                                        string CleanPath = ClensePath.CleanPath(AuthToken, new DirectoryInfo(SyncPath).Name, RelevantPath);

                                        //Return True ( File Renamed ) or False ( File Failed to Rename )
                                        ServerResponse = CloudFunctions.RenameFile(CleanPath, FileName, OldFileName) ? "true" : "false";
                                    }
                                    else
                                    {
                                        ServerResponse = "OldFileName Parameter was not provided";
                                    }
                                }
                                else
                                {
                                    ServerResponse = "FileName Parameter was not provided";
                                }
                            }
                            else
                            {
                                ServerResponse = "FullPath Parameter was not provided";

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
                ServerResponse = "Token parameter was not provided";
            }

            return ServerResponse;
        }

        public static string Delete(TcpClient clientSocket, string data)
        {
            Token AuthToken;
            string Token;
            string SyncPath;
            string RelevantPath;
            string FullPath;
            string FileName;
            string ServerResponse;

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

                            if (Request.Contains("FullPath", data))
                            {
                                FullPath = Request.Get("FullPath", data);

                                if (Request.Contains("FileName", data))
                                {
                                    FileName = Request.Get("FileName", data);

                                    string CleanPath = ClensePath.CleanPath(AuthToken, new DirectoryInfo(SyncPath).Name, RelevantPath);

                                    //Return True ( File Renamed ) or False ( File Failed to Rename )
                                    ServerResponse = CloudFunctions.DeleteFile(CleanPath, FileName) ? "true" : "false";
                                }
                                else
                                {
                                    ServerResponse = "FileName Parameter was not provided";
                                }
                            }
                            else
                            {
                                ServerResponse = "FullPath Parameter was not provided";

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
                ServerResponse = "Token parameter was not provided";
            }

            return ServerResponse;
        }
        #endregion

        #region Comparables
        //Database -> Grab Properties -> Compare Cached Properties
        //Return True == (Client has a good file) || False == (Server has the better file)
        public static string CompareProperties(TcpClient clientSocket, string data)
        {
            Token AuthToken;
            string Token;

            //File Properties
            string Size; //Size Hash
            string Created; //Created Hash
            string LastModified; //Last Modified

            //File Path
            string SyncPath;
            string RelevantPath;
            string FileName;

            string ServerResponse;

            if (Request.Contains("Token", data))
            {
                Token = Request.Get("Token", data);

                //Get Token by Token -> Get Token by IP Address -> Compare Returned Tokens
                if (Tokens.GetTokenByToken(Token) != null && Tokens.GetToken(((IPEndPoint)clientSocket.Client.RemoteEndPoint).Address.ToString()) != null && Tokens.GetTokenByToken(Token) == Tokens.GetToken(((IPEndPoint)clientSocket.Client.RemoteEndPoint).Address.ToString()))
                {
                    AuthToken = Tokens.GetTokenByToken(Token);

                    if (Request.Contains("Size", data))
                    {
                        Size = Request.Get("Size", data);

                        if (Request.Contains("Created", data))
                        {
                            Created = Request.Get("Created", data);

                            if (Request.Contains("SyncPath", data))
                            {
                                SyncPath = Request.Get("SyncPath", data);

                                if (Request.Contains("LastModified", data))
                                {
                                    LastModified = Request.Get("LastModified", data);

                                    if (Request.Contains("RelevantPath", data))
                                    {
                                        RelevantPath = Request.Get("RelevantPath", data);

                                        if (Request.Contains("FileName", data))
                                        {
                                            FileName = Request.Get("FileName", data);

                                            string FullPath = ClensePath.CleanPath(AuthToken, new DirectoryInfo(SyncPath).Name, RelevantPath) + FileName;

                                            if (Convert.ToInt64(Size) != new FileInfo(FullPath).Length)
                                            {
                                                ServerResponse = "Size Different";
                                            }
                                            else if (Convert.ToInt64(Created) != File.GetCreationTimeUtc(FullPath).ToBinary())
                                            {
                                                ServerResponse = "Created Different";
                                            }
                                            else if (Convert.ToInt64(LastModified) != File.GetLastWriteTimeUtc(FullPath).ToBinary())
                                            {
                                                ServerResponse = "LastModified Different";
                                            }
                                            else
                                            {
                                                ServerResponse = "Same";
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
                                    ServerResponse = "LastModified Parameter was not provided";
                                }
                            }
                            else
                            {
                                ServerResponse = "SyncPath Parameter was not provided";
                            }
                        }
                        else
                        {
                            ServerResponse = "Created Parameter was not provided";
                        }
                    }
                    else
                    {
                        ServerResponse = "Size Parameter was not provided";
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

        public static string CompareSections(TcpClient clientSocket, string data)
        {
            Token AuthToken;
            string Token;
            string ServerResponse;

            if (Request.Contains("Token", data))
            {
                Token = Request.Get("Token", data);

                //Get Token by Token -> Get Token by IP Address -> Compare Returned Tokens
                if (Tokens.GetTokenByToken(Token) != null && Tokens.GetToken(((IPEndPoint)clientSocket.Client.RemoteEndPoint).Address.ToString()) != null && Tokens.GetTokenByToken(Token) == Tokens.GetToken(((IPEndPoint)clientSocket.Client.RemoteEndPoint).Address.ToString()))
                {
                    AuthToken = Tokens.GetTokenByToken(Token);

                    ServerResponse = "Not Implemented";
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
        #endregion
    }
}
