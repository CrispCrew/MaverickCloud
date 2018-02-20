using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CloudServer
{
    /// <summary>
    /// The AuthLib API Client
    /// </summary>
    public class API
    {
        /// <summary>
        /// Send API Request to specified clientSocket
        /// <param name="clientSocket">Server Socket</param>
        /// <param name="Request">API Request</param>
        /// </summary>
        public static string SendAPIRequest(NetworkStream serverStream, string Request, bool Response = true)
        {
            //Sockets Connection
            //Debug - Log Times
            Stopwatch timer = new Stopwatch();
            timer.Start();

            byte[] outStream = Encoding.ASCII.GetBytes(Request);
            byte[] outSize = BitConverter.GetBytes(outStream.Length);

            Console.WriteLine("Raw Data: " + BitConverter.ToInt32(outSize, 0) + " -> " + Encoding.ASCII.GetString(outStream));

            //Write Bytes
            serverStream.Write(outSize, 0, outSize.Length);
            serverStream.Write(outStream, 0, outStream.Length);
            serverStream.Flush();

            //Wait for Response - TODO: Add Recieve Byte outSize

            if (Response)
            {
                byte[] size = new byte[4];

                serverStream.Read(size, 0, size.Length);

                byte[] bytesFrom = new byte[BitConverter.ToInt32(size, 0)];

                Console.WriteLine("ExpectedSize: " + BitConverter.ToInt32(size, 0) + " bytesFrom Length: " + bytesFrom.Length);

                serverStream.Read(bytesFrom, 0, bytesFrom.Length);

                string returndata = Encoding.ASCII.GetString(bytesFrom);

                Console.WriteLine("Data from Server: " + returndata);

                Console.WriteLine(timer.Elapsed.TotalMilliseconds + "ms");

                timer.Reset();

                return returndata;
            }
            else
                return "";
        }

        public static bool Contains(string Variable, string Data)
        {
            if (!Data.Contains(Variable + "="))
                return false;

            return true;
        }

        public static string Get(string Variable, string Data)
        {
            if (!Data.Contains(Variable + "="))
                return null;

            string[] posted = Data.Split('&');

            foreach (string post in posted)
            {
                if (post.Contains(Variable + "="))
                {
                    return post.Replace(Variable + "=", "");
                }
            }

            return Data;
        }
    }
}
