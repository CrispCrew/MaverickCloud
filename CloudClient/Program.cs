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
                watcher.NotifyFilter = NotifyFilters.FileName;

                watcher.Error += Watcher_Error;
                watcher.Created += Watcher_Created;
                watcher.Changed += Watcher_Changed;
                watcher.Renamed += Watcher_Renamed;
                watcher.Deleted += Watcher_Deleted;

                watcher.EnableRaisingEvents = true;

                FileSystemWatcher dirwatcher = new FileSystemWatcher();
                dirwatcher.Path = path;
                dirwatcher.IncludeSubdirectories = true;
                dirwatcher.InternalBufferSize = 16384;
                dirwatcher.NotifyFilter = NotifyFilters.DirectoryName;

                dirwatcher.Error += Watcher_Error;
                dirwatcher.Created += Watcher_Created;
                dirwatcher.Changed += Watcher_Changed;
                dirwatcher.Renamed += Watcher_Renamed;
                dirwatcher.Deleted += Watcher_Deleted;

                dirwatcher.EnableRaisingEvents = true;

                Cache.SystemWatchers.Add(watcher);
                Cache.SystemWatchers.Add(dirwatcher);
            }

            while (true)
            {
                //Console.WriteLine(connect.Files(Token));

                foreach (SyncQueue sync in new List<SyncQueue>(Cache.SyncQueue))
                {
                    Console.WriteLine("Processing File: File={" + sync.SyncPath + sync.RelevantPath + sync.FileName + "}");

                    if (!sync.Folder && sync.EventAction != SyncQueue.Action.Deleted)
                        while (IsFileLocked(new FileInfo(sync.FullPath)))
                        {
                            Console.WriteLine(sync.FullPath + " is locked!!!!");

                            Thread.Sleep(1000);
                        }

                    if (sync.EventAction == SyncQueue.Action.Created)
                    {
                        Console.WriteLine("File Created: SyncPath={" + sync.SyncPath + "}, RelevantPath={" + sync.RelevantPath + "}, FileName={" + sync.FileName + "}");

                        if (sync.Folder)
                        {
                            if (connect.CreateFolder(Token, sync.SyncPath, sync.RelevantPath, sync.FileName))
                                Cache.SyncQueue.Remove(sync);
                        }
                        else
                        {
                            //Created
                            if (connect.Upload(Token, sync.SyncPath, sync.RelevantPath, sync.FileName))
                                Cache.SyncQueue.Remove(sync);

                            /*
                            if (connect.CompareProperties(Token, sync.SyncPath, sync.RelevantPath, sync.FileName))
                            {
                                Cache.SyncQueue.Remove(sync);
                            }
                            */
                        }
                    }
                    else if (sync.EventAction == SyncQueue.Action.Changed)
                    {
                        Console.WriteLine("File Changed: SyncPath={" + sync.SyncPath + "}, RelevantPath={" + sync.RelevantPath + "}, FileName={" + sync.FileName + "}");

                        //Changed -> Client Changed File
                        if (connect.Upload(Token, sync.SyncPath, sync.RelevantPath, sync.FileName))
                            Cache.SyncQueue.Remove(sync);

                        /*
                        if (connect.CompareProperties(Token, sync.SyncPath, sync.RelevantPath, sync.FileName))
                        {
                            Cache.SyncQueue.Remove(sync);
                        }
                        */
                    }
                    else if (sync.EventAction == SyncQueue.Action.Renamed)
                    {
                        //Renamed
                        if (connect.Rename(Token, sync.SyncPath, sync.RelevantPath, sync.FileName, sync.OldFileName, sync.Folder))
                            Cache.SyncQueue.Remove(sync);
                    }
                    else if (sync.EventAction == SyncQueue.Action.Deleted)
                    {
                        //Deleted
                        if (connect.Delete(Token, sync.SyncPath, sync.RelevantPath, sync.FileName, sync.Folder))
                            Cache.SyncQueue.Remove(sync);
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
                if (sync.SyncPath == SyncPath && sync.RelevantPath == RelevantPath && sync.FileName == FileName)
                    if (sync.EventAction == SyncQueue.Action.Created)
                        if (sync.LastModified >= File.GetLastWriteTimeUtc(SyncPath + RelevantPath + FileName).ToBinary())
                            return;

            Cache.SyncQueue.Add(new SyncQueue(SyncQueue.Action.Created, SyncPath, RelevantPath, FileName, (File.GetAttributes(e.FullPath) & FileAttributes.Directory) == FileAttributes.Directory));

            Console.WriteLine("[NOTICE] FileSystemWatcher: {Unfiltred=" + e.FullPath + ", SyncPath=" + SyncPath + ", RelevantPath=" + RelevantPath + "}");
        }

        private static void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if ((File.GetAttributes(e.FullPath) & FileAttributes.Directory) == FileAttributes.Directory)
                return;

            Console.WriteLine("[NOTICE] FileSystemWatcher: FileChanged {FileName=" + e.Name + ", FullPath=" + e.FullPath + ", ChangeType=Changed}");

            string SyncPath = "";
            string RelevantPath = "";
            string FileName = e.Name;

            foreach (FileSystemWatcher watcher in Cache.SystemWatchers)
                if (e.FullPath.Contains(watcher.Path))
                    SyncPath = watcher.Path;

            RelevantPath = e.FullPath.Replace(SyncPath, "").Replace(FileName, "");

            foreach (SyncQueue sync in new List<SyncQueue>(Cache.SyncQueue))
                if (sync.SyncPath == SyncPath && sync.RelevantPath == RelevantPath && sync.FileName == FileName)
                    if (sync.EventAction == SyncQueue.Action.Changed)
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
            string FileName = e.Name;
            string OldFileName = e.OldName;

            foreach (FileSystemWatcher watcher in Cache.SystemWatchers)
                if (e.FullPath.Contains(watcher.Path))
                    SyncPath = watcher.Path;

            RelevantPath = e.FullPath.Replace(SyncPath, "").Replace(FileName, "");

            Cache.SyncQueue.Add(new SyncQueue(SyncQueue.Action.Renamed, SyncPath, RelevantPath, FileName, OldFileName, (File.GetAttributes(e.FullPath) & FileAttributes.Directory) == FileAttributes.Directory));

            Console.WriteLine("[NOTICE] FileSystemWatcher: {Unfiltred=" + e.FullPath + ", SyncPath=" + SyncPath + ", RelevantPath=" + RelevantPath + "}");
        }

        private static void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            bool isFolder = false;

            foreach (FileSystemWatcher filewatcher in Cache.SystemWatchers)
                if (sender == filewatcher && (filewatcher.NotifyFilter & NotifyFilters.DirectoryName) == NotifyFilters.DirectoryName)
                    isFolder = true;

            Console.WriteLine("[NOTICE] FileSystemWatcher: FileDeleted {FileName=" + e.Name + ", FullPath=" + e.FullPath + ", ChangeType=Deleted, FileType=" + (isFolder ? "Folder" : "File") + "}");

            string SyncPath = "";
            string RelevantPath = "";
            string FileName = e.Name;

            foreach (FileSystemWatcher watcher in Cache.SystemWatchers)
                if (e.FullPath.Contains(watcher.Path))
                    SyncPath = watcher.Path;

            RelevantPath = e.FullPath.Replace(SyncPath, "").Replace(FileName, "");

            Cache.SyncQueue.Add(new SyncQueue(SyncQueue.Action.Deleted, SyncPath, RelevantPath, FileName, isFolder));

            Console.WriteLine("[NOTICE] FileSystemWatcher: {Unfiltred=" + e.FullPath + ", SyncPath=" + SyncPath + ", RelevantPath=" + RelevantPath + "}");
        }
        #endregion

        private static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch
            {
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            return false;
        }
    }
}
