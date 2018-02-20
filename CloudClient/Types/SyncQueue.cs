using System.IO;
using System.Linq;

namespace CloudClient
{
    public class SyncQueue
    {
        public enum Action
        {
            Created,
            Changed,
            Renamed,
            Deleted
        };

        public Action EventAction;
        public string SyncPath;
        public string RelevantPath;
        public string FileName;
        public string NewName;
        public bool Folder;

        public string FullPath;
        public long Created;
        public long LastModified;
        public long Size;

        /// <summary>
        /// Rename File
        /// </summary>
        /// <param name="EventAction"></param>
        /// <param name="SyncPath"></param>
        /// <param name="RelevantPath"></param>
        /// <param name="FileName"></param>
        /// <param name="NewName"></param>
        public SyncQueue(Action EventAction, string SyncPath, string RelevantPath, string FileName, bool Folder)
        {
            this.EventAction = EventAction;
            this.SyncPath = SyncPath;
            this.RelevantPath = RelevantPath;
            this.FileName = FileName;
            this.Folder = Folder;

            if (Folder)
            {
                FullPath = SyncPath + RelevantPath + NewName;
                Created = Directory.GetCreationTimeUtc(FullPath).ToBinary();
                LastModified = Directory.GetLastWriteTimeUtc(FullPath).ToBinary();
                Size = new DirectoryInfo(FullPath).EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length);
            }
            else
            {
                FullPath = SyncPath + RelevantPath + FileName;
                Created = File.GetCreationTimeUtc(FullPath).ToBinary();
                LastModified = File.GetLastWriteTimeUtc(FullPath).ToBinary();
                Size = new FileInfo(FullPath).Length;
            }
        }

        /// <summary>
        /// Rename File
        /// </summary>
        /// <param name="EventAction"></param>
        /// <param name="SyncPath"></param>
        /// <param name="RelevantPath"></param>
        /// <param name="FileName"></param>
        /// <param name="NewName"></param>
        public SyncQueue(Action EventAction, string SyncPath, string RelevantPath, string FileName, string NewName, bool Folder)
        {
            this.EventAction = EventAction;
            this.SyncPath = SyncPath;
            this.RelevantPath = RelevantPath;
            this.FileName = FileName;
            this.NewName = NewName;
            this.Folder = Folder;

            if (Folder)
            {
                FullPath = SyncPath + RelevantPath + NewName;
                Created = Directory.GetCreationTimeUtc(FullPath).ToBinary();
                LastModified = Directory.GetLastWriteTimeUtc(FullPath).ToBinary();
                Size = new DirectoryInfo(FullPath).EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length);
            }
            else
            {
                FullPath = SyncPath + RelevantPath + FileName;
                Created = File.GetCreationTimeUtc(FullPath).ToBinary();
                LastModified = File.GetLastWriteTimeUtc(FullPath).ToBinary();
                Size = new FileInfo(FullPath).Length;
            }
        }
    }
}
