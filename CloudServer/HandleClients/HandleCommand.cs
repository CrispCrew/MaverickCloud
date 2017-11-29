using CloudServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CloudServer.HandleClients
{
    public class HandleCommand
    {
        public static string ParseCommand(TcpClient clientSocket, string data)
        {
            string Function;
            string ServerResponse;

            //Request Variable
            if (Request.Contains("Request", data))
            {
                Function = Request.Get("Request", data);

                Console.WriteLine(Function);

                #region User API Requests

                if (Function == "Version")
                    ServerResponse = "0.00";
                else if (Function == "Login")
                    ServerResponse = HandleAccount.Login(clientSocket, data);
                else if (Function == "Upload")
                    ServerResponse = HandleFiles.DownloadFile(clientSocket, data);
                else if (Function == "Download")
                    ServerResponse = HandleFiles.UploadFile(clientSocket, data);
                else
                    ServerResponse = "Undefined Request";
                #endregion
            }
            else
            {
                ServerResponse = "Invalid API Request";
            }

            Console.WriteLine("Server Response: " + ServerResponse + " Data: " + data + " IP -> " + ((IPEndPoint)clientSocket.Client.RemoteEndPoint).Address.ToString());

            return ServerResponse;
        }
    }
}
