using System.Collections.Generic;
using System.IO;

namespace CloudClient
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
