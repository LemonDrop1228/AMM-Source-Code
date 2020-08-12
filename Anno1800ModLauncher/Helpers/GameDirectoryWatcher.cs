using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Anno1800ModLauncher.Helpers
{
    public class GameDirectoryWatcher
    {
        private FileSystemWatcher watcher { get; set; }
        

        public delegate void GameDirectoryUpdatedEventHandler();
        public event GameDirectoryUpdatedEventHandler GameDirectoryUpdated;

        public GameDirectoryWatcher(string pathToWatch)
        {
            watcher = CreateWatcher(pathToWatch);
            watcher.Changed += (s,e) => { Watcher_Changed(e); };
            watcher.Deleted += (s, e) => { Watcher_Deleted(e); };
            watcher.Created += (s, e) => { Watcher_Created(e); };
            watcher.EnableRaisingEvents = true;
        }

        private void Watcher_Created(FileSystemEventArgs e)
        {
            string modDir = Path.Combine(Properties.Settings.Default.GameRootPath, Properties.Settings.Default.ModDirectory);
            if (e.FullPath == modDir)
                GameDirectoryUpdated();
        }

        private void Watcher_Deleted(FileSystemEventArgs e)
        {
            string modDir = Path.Combine(Properties.Settings.Default.GameRootPath, Properties.Settings.Default.ModDirectory);
            if (e.Name.Contains("python35_ubi.dll")
                || e.Name.Contains("python35.dll")
                || e.Name == "mods")
                GameDirectoryUpdated();
        }

        private void Watcher_Changed(FileSystemEventArgs e)
        {
            string modDir = Path.Combine(Properties.Settings.Default.GameRootPath, Properties.Settings.Default.ModDirectory);
            if (e.Name.Contains("python35_ubi.dll"))
                GameDirectoryUpdated();
        }

        private FileSystemWatcher CreateWatcher(string pathToWatch)
        {
            return new FileSystemWatcher
            {
                Path = pathToWatch,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.CreationTime,
                IncludeSubdirectories = true,
                Filter = "*"
            };
        }

        public void Dispose()
        {
            watcher.Dispose();
        }
    }
}
