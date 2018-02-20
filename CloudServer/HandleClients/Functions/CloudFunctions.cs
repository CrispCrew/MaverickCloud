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
        public static bool RenameFile(string FullPath, string FileName, string OldFileName, bool Folder)
        {
            if (!Folder)
                File.Move(FullPath + OldFileName, FullPath + FileName);
            else
                Directory.Move(FullPath + OldFileName, FullPath + FileName);

            return true;
        }

        public static bool DeleteFile(string FullPath, string FileName, bool Folder)
        {
            if (!Folder)
                File.Delete(FullPath + FileName);
            else
                Directory.Delete(FullPath + FileName, true);

            return true;
        }
    }
}
