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
    public class HandleFolders
    {
        public static string Create(TcpClient clientSocket, string data)
        {
            Token AuthToken;
            string Token, SyncPath, RelevantPath, FileName;
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

                                string FullPath = ClensePath.CleanPath(AuthToken, SyncPath, RelevantPath) + FileName;

                                Console.WriteLine("Creating Folder: " + FullPath);

                                if (!Directory.Exists(FullPath))
                                {
                                    Directory.CreateDirectory(FullPath);

                                    ServerResponse = "Folder Created";
                                }
                                else
                                {
                                    ServerResponse = "Folder Exists";
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
