using System;
using System.Collections.Generic;
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
            //Cache.Folders.Add(UserPath);

            Cache.Folders.Add(UserPath);

            //Console Title
            Console.Title = "Maverick Logs";

            connect.Version();

            connect.Login("CrispyCheats", "test", out Token);

            //new Thread(ServerHealth).Start();

            //connect.Upload(Token, "C:\\Users\\Nitro\\MaverickCloud", "NewFolder", "File.txt");

            foreach (string path in Cache.Folders)
            {
                Console.WriteLine("Event Handler Initializing -> " + path);

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                Console.WriteLine("[NOTICE] FileSystemWatcher: Adding Watcher {Path=" + path + "}");

                FileSystemWatcher watcher = new FileSystemWatcher();
                watcher.Path = path;
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

            while (true)
            {
                Console.WriteLine(connect.Files(Token));

                foreach (SyncQueue sync in new List<SyncQueue>(Cache.SyncQueue))
                {
                    Console.WriteLine("Processing File: File={" + sync.SyncPath + sync.RelevantPath + sync.FileName + "}");

                    if (sync.EventAction == SyncQueue.Action.Created)
                    {
                        Console.WriteLine("File Created: SyncPath={" + sync.SyncPath + "}, RelevantPath={" + sync.RelevantPath + "}, FileName={" + sync.FileName + "}");

                        if (sync.Folder)
                        {

                        }
                        else
                        {
                            //Created
                            connect.Upload(Token, sync.SyncPath, sync.RelevantPath, sync.FileName);

                            if (connect.CompareProperties(Token, sync.SyncPath, sync.RelevantPath, sync.FileName))
                            {
                                Cache.SyncQueue.Remove(sync);
                            }
                        }
                    }
                    else if (sync.EventAction == SyncQueue.Action.Changed)
                    {
                        Console.WriteLine("File Changed: SyncPath={" + sync.SyncPath + "}, RelevantPath={" + sync.RelevantPath + "}, FileName={" + sync.FileName + "}");

                        //Changed -> Client Changed File
                        connect.Upload(Token, sync.SyncPath, sync.RelevantPath, sync.FileName);

                        if (connect.CompareProperties(Token, sync.SyncPath, sync.RelevantPath, sync.FileName))
                        {
                            Cache.SyncQueue.Remove(sync);
                        }
                    }
                    else if (sync.EventAction == SyncQueue.Action.Renamed)
                    {
                        //Renamed
                        //connect.Rename(Token, sync.SyncPath, sync.RelevantPath, sync.FileName);
                    }
                    else if (sync.EventAction == SyncQueue.Action.Deleted)
                    {
                        //Deleted
                        //connect.Delete(Token, sync.SyncPath, sync.RelevantPath, sync.FileName);
                    }
                }

                //Check Current Files for Server Side Changes

                Thread.Sleep(250);
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
            string RelevantPath = "";
            string FileName = e.Name;

            foreach (FileSystemWatcher watcher in Cache.SystemWatchers)
                if (e.FullPath.Contains(watcher.Path))
                    SyncPath = watcher.Path;

            RelevantPath = e.FullPath.Replace(SyncPath, "").Replace(FileName, "");

            foreach (SyncQueue sync in new List<SyncQueue>(Cache.SyncQueue))
                if (sync.EventAction == SyncQueue.Action.Created)
                    if (sync.LastModified >= File.GetLastWriteTimeUtc(SyncPath + RelevantPath + FileName).ToBinary())
                        return;

            Cache.SyncQueue.Add(new SyncQueue(SyncQueue.Action.Created, SyncPath, RelevantPath, FileName, (File.GetAttributes(e.FullPath) & FileAttributes.Directory) == FileAttributes.Directory));

            Console.WriteLine("[NOTICE] FileSystemWatcher: {Unfiltred=" + e.FullPath + ", SyncPath=" + SyncPath + ", RelevantPath=" + RelevantPath + "}");
        }

        private static void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("[NOTICE] FileSystemWatcher: FileChanged {FileName=" + e.Name + ", FullPath=" + e.FullPath + ", ChangeType=Changed}");

            string SyncPath = "";
            string RelevantPath = "";
            string FileName = e.Name;

            foreach (FileSystemWatcher watcher in Cache.SystemWatchers)
                if (e.FullPath.Contains(watcher.Path))
                    SyncPath = watcher.Path;

            RelevantPath = e.FullPath.Replace(SyncPath, "").Replace(FileName, "");

            foreach (SyncQueue sync in new List<SyncQueue>(Cache.SyncQueue))
                if (sync.EventAction == SyncQueue.Action.Created)
                    if (sync.LastModified >= File.GetLastWriteTimeUtc(SyncPath + RelevantPath + FileName).ToBinary())
                        return;

            Cache.SyncQueue.Add(new SyncQueue(SyncQueue.Action.Changed, SyncPath, RelevantPath, FileName, (File.GetAttributes(e.FullPath) & FileAttributes.Directory) == FileAttributes.Directory));

            Console.WriteLine("[NOTICE] FileSystemWatcher: {Unfiltred=" + e.FullPath + ", SyncPath=" + SyncPath + ", RelevantPath=" + RelevantPath + "}");
        }

        private static void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            Console.WriteLine("[NOTICE] FileSystemWatcher: FileRenamed {FileName=" + e.Name + ", FullPath=" + e.FullPath + ", ChangeType=Renamed}");

            string SyncPath = "";
            string RelevantPath = "";
            string FileName = e.OldName;
            string NewName = e.Name;

            foreach (FileSystemWatcher watcher in Cache.SystemWatchers)
                if (e.FullPath.Contains(watcher.Path))
                    SyncPath = watcher.Path;

            RelevantPath = e.FullPath.Replace(SyncPath, "").Replace(FileName, "");

            Cache.SyncQueue.Add(new SyncQueue(SyncQueue.Action.Renamed, SyncPath, RelevantPath, FileName, NewName, (File.GetAttributes(e.FullPath) & FileAttributes.Directory) == FileAttributes.Directory));

            Console.WriteLine("[NOTICE] FileSystemWatcher: {Unfiltred=" + e.FullPath + ", SyncPath=" + SyncPath + ", RelevantPath=" + RelevantPath + "}");
        }

        private static void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("[NOTICE] FileSystemWatcher: FileDeleted {FileName=" + e.Name + ", FullPath=" + e.FullPath + ", ChangeType=Deleted}");

            string SyncPath = "";
            string RelevantPath = "";
            string FileName = e.Name;

            foreach (FileSystemWatcher watcher in Cache.SystemWatchers)
                if (e.FullPath.Contains(watcher.Path))
                    SyncPath = watcher.Path;

            RelevantPath = e.FullPath.Replace(SyncPath, "").Replace(FileName, "");

            Cache.SyncQueue.Add(new SyncQueue(SyncQueue.Action.Deleted, SyncPath, RelevantPath, FileName, (File.GetAttributes(e.FullPath) & FileAttributes.Directory) == FileAttributes.Directory));

            Console.WriteLine("[NOTICE] FileSystemWatcher: {Unfiltred=" + e.FullPath + ", SyncPath=" + SyncPath + ", RelevantPath=" + RelevantPath + "}");
        }
        #endregion
    }
}
