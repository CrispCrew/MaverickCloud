using System;
using System.IO;
using System.Threading;

namespace CloudClient
{
    public class Program
    {
        /*
        Copyright Protected by James Matthews, MaverickCheats and MaverickCloud (2017)
        */

        public static string UserPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\MaverickCloud\\";

        public static Client connect = new Client();

        public static string Token = "";

        public static void Main(string[] args)
        {
            Cache.Folders.Add(UserPath);

            //Console Title
            Console.Title = "Maverick Logs";

            connect.Version();

            connect.Login("CrispyCheats", "test", out Token);

            new Thread(ServerHealth).Start();

            //connect.Upload(Token, "", "C:\\File.txt", "File.txt");

            //connect.Download(Token, "", "C:\\File.txt", "File.txt");

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

        private static void ServerHealth()
        {
            while (true)
            {
                connect.APICheck(Token);

                Thread.Sleep(10000);
            }
        }

        #region FileSystemWatcher EventHandlers
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

            Console.WriteLine("[NOTICE] FileSystemWatcher: {Unfiltred=" + FullPath + ", SyncPath=" + SyncPath + ", RelevantPath=" + FolderPath + "}");
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

            Console.WriteLine("[NOTICE] FileSystemWatcher: {Unfiltred=" + FullPath + ", SyncPath=" + SyncPath + ", RelevantPath=" + FolderPath + "}");
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

            Console.WriteLine("[NOTICE] FileSystemWatcher: {Unfiltred=" + FullPath + ", SyncPath=" + SyncPath + ", RelevantPath=" + FolderPath + "}");
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

            Console.WriteLine("[NOTICE] FileSystemWatcher: {Unfiltred=" + FullPath + ", SyncPath=" + SyncPath + ", RelevantPath=" + FolderPath + "}");
        }
        #endregion
    }
}
