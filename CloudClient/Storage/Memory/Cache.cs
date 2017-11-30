using CloudClient.Types;
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
        //All SyncFolders which are actively syncing
        public static List<string> Folders = new List<string>();

        //FileSystemWatchers which are active
        public static List<FileSystemWatcher> SystemWatchers = new List<FileSystemWatcher>();
        
        //Files waiting to Sync
        public static List<SyncQueue> SyncQueue = new List<SyncQueue>();
    }
}
