﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudServer.HandleClients
{
    public class ClensePath
    {
        public static string CleanPath(Token AuthToken)
        {
            return Environment.CurrentDirectory + @"\Users\" + AuthToken.Username + @"\";
        }

        public static string CleanPath(Token AuthToken, string SyncFolder)
        {
            return Environment.CurrentDirectory + @"\Users\" + AuthToken.Username + @"\" + new DirectoryInfo(SyncFolder).Name + @"\";
        }

        public static string CleanPath(Token AuthToken, string SyncFolder, string RelevantPath)
        {
            return Environment.CurrentDirectory + @"\Users\" + AuthToken.Username + @"\" + new DirectoryInfo(SyncFolder).Name + @"\" + (RelevantPath != "" ? RelevantPath + @"\" : "");
        }
    }
}
