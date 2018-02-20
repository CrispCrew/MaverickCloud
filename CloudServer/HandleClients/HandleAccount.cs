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
    public class HandleAccount
    {
        public static string Login(TcpClient clientSocket, string data)
        {
            string Username;
            string Password;
            string ServerResponse;

            if (Request.Contains("Username", data))
            {
                Username = Request.Get("Username", data);

                if (Request.Contains("Password", data))
                {
                    Password = Request.Get("Password", data);

                    if (Username == "CrispyCheats" && Password == "test")
                    {
                        ServerResponse = "Login Found" + "-" + Tokens.GenerateToken(((IPEndPoint)clientSocket.Client.RemoteEndPoint).Address.ToString(), 1, Username);

                        if (!Directory.Exists(Environment.CurrentDirectory + "\\Users\\" + Username + "\\"))
                            Directory.CreateDirectory(Environment.CurrentDirectory + "\\Users\\" + Username + "\\");
                    }
                    else
                    {
                        ServerResponse = "Login not Found";
                    }
                }
                else
                {
                    ServerResponse = "Password Parameter was not provided";
                }
            }
            else
            {
                ServerResponse = "Username Parameter was not provided";
            }

            return ServerResponse;
        }
    }
}
