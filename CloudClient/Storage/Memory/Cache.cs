using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudClient.Storage
{
    public class Cache
    {
        public static List<string> Folders = new List<string>();
        public static List<FileSystemWatcher> SystemWatchers = new List<FileSystemWatcher>();
    }
}
