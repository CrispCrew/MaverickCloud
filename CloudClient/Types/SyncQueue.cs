using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudClient.Types
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
        public string FullPath;
        public string Path;

        /// <summary>
        /// Create, Change and Delete File
        /// </summary>
        /// <param name="EventAction"></param>
        /// <param name="SyncPath"></param>
        /// <param name="FullPath"></param>
        /// <param name="Path"></param>
        /// <param name="Name"></param>
        public SyncQueue(Action EventAction, string SyncPath, string FullPath, string FolderPath, string Name)
        {
             
        }

        /// <summary>
        /// Rename File
        /// </summary>
        /// <param name="EventAction"></param>
        /// <param name="SyncPath"></param>
        /// <param name="FullPath"></param>
        /// <param name="Path"></param>
        /// <param name="Name"></param>
        /// <param name="NewName"></param>
        public SyncQueue(Action EventAction, string SyncPath, string FullPath, string FolderPath, string Name, string NewName)
        {

        }
    }
}
