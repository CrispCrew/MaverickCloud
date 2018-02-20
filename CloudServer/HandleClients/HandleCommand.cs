using CloudServer;
using CloudServer.Database;
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
                    ServerResponse = HandleVersion.ServerVersion();
                else if (Function == "Login")
                    ServerResponse = HandleAccount.Login(clientSocket, data);
                else if (Function == "APICheck")
                    ServerResponse = HandleAPICheck.APICheck(clientSocket, data);
                else if (Function == "Files")
                    ServerResponse = HandleFiles.Files(clientSocket, data);
                else if (Function == "CreateFolder")
                    ServerResponse = HandleFolders.Create(clientSocket, data);
                else if (Function == "Upload")
                    ServerResponse = HandleFiles.DownloadFile(clientSocket, data);
                else if (Function == "Download")
                    ServerResponse = HandleFiles.UploadFile(clientSocket, data);
                else if (Function == "Rename")
                    ServerResponse = HandleCloud.Rename(clientSocket, data);
                else if (Function == "Delete")
                    ServerResponse = HandleCloud.Delete(clientSocket, data);
                //Recieves Comparable Properties ( Size, Dates...etc ) and compares it to the Server Properties
                else if (Function == "CompareProperties")
                    ServerResponse = HandleCloud.CompareProperties(clientSocket, data);
                //Recieves Section Hashes which is then compared to the Server Section Hashes
                else if (Function == "CompareSections")
                    ServerResponse = HandleCloud.CompareSections(clientSocket, data);
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
