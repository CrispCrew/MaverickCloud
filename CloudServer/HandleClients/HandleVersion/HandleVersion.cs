using CloudServer.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudServer.HandleClients
{
    public class HandleVersion
    {
        public static string ServerVersion()
        {
            string Version;

            Connect connect = new Connect();

            Version = connect.Version().ToString();

            connect.Close();

            return Version;
        }
    }
}
