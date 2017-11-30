using CloudClient.Storage;
using CloudClient.Types;
using System;
using System.IO;

namespace CloudClient
{
    public class Program
    {
        public static string UserPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\MaverickCloud\\";
        public static string CustomPath = Environment.CurrentDirectory + "\\MaverickCloud\\";

        public static void Main(string[] args)
        {
            Cache.Folders.Add(UserPath);
            Cache.Folders.Add(CustomPath);

            string Token = null;

            //Console Title
            Console.Title = "Maverick Logs";

            Client connect = new Client();

            connect.Version();

            connect.Login("CrispyCheats", "test", out Token);

            connect.Upload(Token, "", "C:\\File.txt", "File.txt");

            connect.Download(Token, "", "C:\\File.txt", "File.txt");

            foreach (string path in Cache.Folders)
            {
                if (!Directory.Exists(Path.GetDirectoryName(path)))
                    Directory.CreateDirectory(Path.GetDirectoryName(path));

                Console.WriteLine("[NOTICE] FileSystemWatcher: Adding Watcher {Path=" + Path.GetDirectoryName(path) + "}");

                FileSystemWatcher watcher = new FileSystemWatcher();
                watcher.Path = Path.GetDirectoryName(path);
                watcher.IncludeSubdirectories = true;
                watcher.InternalBufferSize = 16384;

                watcher.Error += Watcher_Error;
                watcher.Created += Watcher_Created;
                watcher.Changed += Watcher_Changed;
                watcher.Renamed += Watcher_Renamed;
                watcher.Deleted += Watcher_Deleted;

                watcher.EnableRaisingEvents = true;

                Cache.SystemWatchers.Add(watcher);
            }

            Console.ReadLine();
        }

        private static void Watcher_Error(object sender, ErrorEventArgs e)
        {
            Console.WriteLine("[ERROR] FileSystemWatcher: " + e.ToString());
        }

        private static void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("[NOTICE] FileSystemWatcher: FileCreated {FileName=" + e.Name + ", FullPath=" + e.FullPath + ", ChangeType=Created}");

            string SyncPath = "";
            string FullPath = e.FullPath;
            string FolderPath = "";
            string Name = e.Name;

            foreach (FileSystemWatcher watcher in Cache.SystemWatchers)
                if (e.FullPath.Contains(watcher.Path))
                    SyncPath = watcher.Path;

            FolderPath = Path.GetDirectoryName(e.FullPath).Replace(SyncPath, "");

            Cache.SyncQueue.Add(new SyncQueue(SyncQueue.Action.Created, SyncPath, FullPath, FolderPath, Name));

            Console.WriteLine("[NOTICE] FileSystemWatcher: {Unfiltred=" + FullPath + ", SyncPath=" + SyncPath + ", Filtered=" + FolderPath + "}");
        }

        private static void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("[NOTICE] FileSystemWatcher: FileChanged {FileName=" + e.Name + ", FullPath=" + e.FullPath + ", ChangeType=Changed}");

            string SyncPath = "";
            string FullPath = e.FullPath;
            string FolderPath = "";
            string Name = e.Name;

            foreach (FileSystemWatcher watcher in Cache.SystemWatchers)
                if (e.FullPath.Contains(watcher.Path))
                    SyncPath = watcher.Path;

            FolderPath = Path.GetDirectoryName(e.FullPath).Replace(SyncPath, "");

            Cache.SyncQueue.Add(new SyncQueue(SyncQueue.Action.Changed, SyncPath, FullPath, FolderPath, Name));

            Console.WriteLine("[NOTICE] FileSystemWatcher: {Unfiltred=" + FullPath + ", SyncPath=" + SyncPath + ", Filtered=" + FolderPath + "}");
        }

        private static void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            Console.WriteLine("[NOTICE] FileSystemWatcher: FileRenamed {FileName=" + e.Name + ", FullPath=" + e.FullPath + ", ChangeType=Renamed}");

            string SyncPath = "";
            string FullPath = e.FullPath;
            string FolderPath = "";
            string Name = e.OldName;
            string NewName = e.Name;

            foreach (FileSystemWatcher watcher in Cache.SystemWatchers)
                if (e.FullPath.Contains(watcher.Path))
                    SyncPath = watcher.Path;

            FolderPath = Path.GetDirectoryName(e.FullPath).Replace(SyncPath, "");

            Cache.SyncQueue.Add(new SyncQueue(SyncQueue.Action.Renamed, SyncPath, FullPath, FolderPath, Name, NewName));

            Console.WriteLine("[NOTICE] FileSystemWatcher: {Unfiltred=" + FullPath + ", SyncPath=" + SyncPath + ", Filtered=" + FolderPath + "}");
        }

        private static void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("[NOTICE] FileSystemWatcher: FileDeleted {FileName=" + e.Name + ", FullPath=" + e.FullPath + ", ChangeType=Deleted}");

            string SyncPath = "";
            string FullPath = e.FullPath;
            string FolderPath = "";
            string Name = e.Name;

            foreach (FileSystemWatcher watcher in Cache.SystemWatchers)
                if (e.FullPath.Contains(watcher.Path))
                    SyncPath = watcher.Path;

            FolderPath = Path.GetDirectoryName(e.FullPath).Replace(SyncPath, "");

            Cache.SyncQueue.Add(new SyncQueue(SyncQueue.Action.Deleted, SyncPath, FullPath, FolderPath, Name));

            Console.WriteLine("[NOTICE] FileSystemWatcher: {Unfiltred=" + FullPath + ", SyncPath=" + SyncPath + ", Filtered=" + FolderPath + "}");
        }

        private static string GetRightPartOfPath(string path, string startAfterPart)
        {
            // use the correct seperator for the environment
            var pathParts = path.Split(Path.DirectorySeparatorChar);

            // this assumes a case sensitive check. If you don't want this, you may want to loop through the pathParts looking
            // for your "startAfterPath" with a StringComparison.OrdinalIgnoreCase check instead
            int startAfter = Array.IndexOf(pathParts, startAfterPart);

            if (startAfter == -1)
            {
                // path path not found
                return null;
            }

            // try and work out if last part was a directory - if not, drop the last part as we don't want the filename
            var lastPartWasDirectory = pathParts[pathParts.Length - 1].EndsWith(Path.DirectorySeparatorChar.ToString());
            return string.Join(
                Path.DirectorySeparatorChar.ToString(),
                pathParts, startAfter,
                pathParts.Length - startAfter - (lastPartWasDirectory ? 0 : 1));
        }
    }
}
