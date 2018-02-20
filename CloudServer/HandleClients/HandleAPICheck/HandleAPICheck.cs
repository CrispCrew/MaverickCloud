using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CloudServer.HandleClients
{
    public class HandleAPICheck
    {
        public static string APICheck(TcpClient clientSocket, string data)
        {
            string Token;
            string ServerResponse;

            if (Request.Contains("Token", data))
            {
                Token = Request.Get("Token", data);

                //Get Token by Token -> Get Token by IP Address -> Compare Returned Tokens
                if (Tokens.GetTokenByToken(Token) != null && Tokens.GetToken(((IPEndPoint)clientSocket.Client.RemoteEndPoint).Address.ToString()) != null && Tokens.GetTokenByToken(Token) == Tokens.GetToken(((IPEndPoint)clientSocket.Client.RemoteEndPoint).Address.ToString()))
                    ServerResponse = "Authenticated";
                else
                    ServerResponse = "Not Authenticated";
            }
            else
            {
                ServerResponse = "Token Parameter was not provided";
            }

            return ServerResponse;
        }
    }
}
