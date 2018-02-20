using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudServer.HandleClients.Functions
{
    public class CloudFunctions
    {
        public static bool RenameFile(string FullPath, string FileName, string OldFileName)
        {
            File.Move(FullPath + FileName, FullPath + OldFileName);

            return true;
        }

        public static bool DeleteFile(string FullPath, string FileName)
        {
            File.Delete(FullPath + FileName);

            return true;
        }
    }
}
