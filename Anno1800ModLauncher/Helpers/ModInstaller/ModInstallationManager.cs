using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Ionic.Zip;
using Ionic.Zlib;
using Microsoft.VisualBasic.FileIO;

namespace Anno1800ModLauncher.Helpers.ModInstaller
{
    public class ModInstallationManager : INotifyPropertyChanged
    {
        private string sourcePath;
        private ObservableCollection<FileItem> fileNames;
        private bool isMulti;

        #region Public Properties
        public string DestinationPath { get; set; } = Path.Combine(Properties.Settings.Default.GameRootPath, Properties.Settings.Default.ModDirectory);
        public string SourcePath { get => sourcePath; private set => sourcePath = CompileArchive(value); }
        public ObservableCollection<FileItem> FileNames { get => fileNames; set => CheckMulti(value); }
        public bool RemoveAfterInstall { get; set; } = true;
        public bool BackupAfterInstall { get; set; }
        #endregion


        #region Private Properties
        private ZipFile ModArchive { get; set; }
        private Version CurrentModVersion { get; set; }
        private Version NewVersion { get; set; }
        private string ModDescription { get; set; }
        private string ModCategory { get; set; }
        #endregion


        #region INotifyPropertyChanged Members
        /// <summary>
        /// Raises the PropertyChanged notification in a thread safe manner
        /// </summary>
        /// <param name="propertyName"></param>
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public delegate void InstallationCompleteEventHandler(MessageBoxResult Result);
        public event InstallationCompleteEventHandler InstallationComplete;


        public ModInstallationManager(string sourcePath)
        {
            SourcePath = sourcePath;
        }

        internal void ActivateAll()
        {
            foreach (var item in FileNames)
            {
                item.IsSelected = true;
            }
        }

        public void Install()
        {
            Task.Run(async () => await InstallAsync().ConfigureAwait(true)).ContinueWith((e) => InstallationComplete(e.Result));
        }

        private async Task<MessageBoxResult> InstallAsync()
        {
            var msgRes = MessageBoxResult.OK;
            try
            {
                if (isMulti)
                {
                    InstallMulti();
                }
                else
                {
                    InstallSingle(ModArchive, DestinationPath);
                }
            }
            catch (Exception ex)
            {
                msgRes = MessageBoxResult.Cancel;
                Console.WriteLine(ex);
            }


            return msgRes;
        }

        /// <summary>
        /// Installs mods from a single zip archive. 
        /// 
        /// If the mod in question is already in the mods folder, it will automatically overwrite and take over the activated status. 
        /// </summary>
        /// <param name="modArchive"></param>
        /// <param name="destinationPath"></param>
        private void InstallSingle(ZipFile modArchive, string destinationPath)
        {
            //temporary directory path for all unpacks
            string TmpDirPath = Path.Combine(Properties.Settings.Default.GameRootPath, "amm_unpack");
            //temporary unpack directory path
            string TmpModPath = Path.Combine(TmpDirPath, modArchive.Name.Split('\\')[modArchive.Name.Split('\\').Length - 1]);
            string modDir = Path.Combine(Properties.Settings.Default.GameRootPath, Properties.Settings.Default.ModDirectory);

            //in case it doesn't already exists, create the unpack directory.
            if (!Directory.Exists(TmpDirPath)) {
                Directory.CreateDirectory(TmpDirPath);
            }

            //extract to the temporary directory
            modArchive.ExtractAll(TmpModPath, ExtractExistingFileAction.OverwriteSilently);

            //enumerate mods in the directory
            var directories = Directory.EnumerateDirectories(TmpModPath);
            //for each mod in the directory, check wether a mod with the same folder name is already installed. currently just don't copy in this case.
            foreach (string s in directories) {
                //get folder name alone
                string folderName = s.Split('\\')[s.Split('\\').Length - 1];
                string folderNameTrimmed = folderName.TrimStart('-').TrimStart(' ');

                //determine what should happen if installation would lead to mod duplications
                if (
                    //no directory of the same name exists
                    !(Directory.Exists(Path.Combine(modDir, folderNameTrimmed)) ||
                    //no directory with the same name but deactivated exists.
                    Directory.Exists(Path.Combine(modDir, "-" + folderNameTrimmed)))
                    )
                {
                    FileSystem.MoveDirectory(s, Path.Combine(modDir, folderName));
                    Console.WriteLine("No mod duplication problems found, copying {0}", folderNameTrimmed);
                }
                else if (
                    //activated directory exists
                    Directory.Exists(Path.Combine(modDir, folderNameTrimmed))
                    )
                {
                    //delete the old one, paste in the new one, activate the new one
                    FileSystem.DeleteDirectory(Path.Combine(modDir, folderNameTrimmed), DeleteDirectoryOption.DeleteAllContents);
                    FileSystem.MoveDirectory(s, Path.Combine(modDir, folderNameTrimmed));
                    Console.WriteLine("Mod Duplication found: replacing", folderNameTrimmed);
                }
                else if (
                    //deactivated directory exists
                    Directory.Exists(Path.Combine(modDir, "-" + folderNameTrimmed))
                    ) 
                {
                    //delete the old one, paste in the new one, deactivate the new one
                    FileSystem.DeleteDirectory(Path.Combine(modDir, "-" + folderNameTrimmed), DeleteDirectoryOption.DeleteAllContents);
                    FileSystem.MoveDirectory(s, Path.Combine(modDir, "-" + folderNameTrimmed));
                    Console.WriteLine("Mod Duplication found: replacing", "-"+folderNameTrimmed);
                }
            }

            //delete temp directories - note: TmpDirPath can be used by other unpacks at the same time, so ONLY delete it if there are no files in it, which is default behavior for Directory.Delete method.
                Directory.Delete(TmpModPath);
                Directory.Delete(TmpDirPath);

            //reload mods
            ModDirectoryManager.Instance.LoadMods();
        }

        private void InstallMulti()
        {
            foreach (var item in FileNames)
            {
                SourcePath = item.ModPath;
                InstallSingle(ModArchive, DestinationPath);
            }
        }

        public ModInstallationManager(string[] fileNames)
        {
            this.FileNames = GetCollection(fileNames);
        }

        private ObservableCollection<FileItem> GetCollection(string[] fileNames)
        {
            var oc = new ObservableCollection<FileItem>();
            foreach (var _string in fileNames)
            {
                oc.Add(GetNewFileItem(_string));
            }
            return oc;
        }

        private FileItem GetNewFileItem(string _string)
        {
            return new FileItem(_string);
        }

        private string CompileArchive(string value)
        {
            ModArchive = new ZipFile(value);
            return value;
        }

        private void CheckMulti(ObservableCollection<FileItem> value)
        {
            if(value.Count() > 1)
            {
                isMulti = true;
                fileNames = value;
                OnPropertyChanged("FileNames");
            }
            else
            {
                fileNames = value;
                SourcePath = value.First().ModPath;
                OnPropertyChanged("FileNames");
            }
        }

    }

    public class FileItem : INotifyPropertyChanged
    {
        private string name;
        private string description;
        private bool isSelected;
        private string modPath;

        public FileItem(string _string)
        {
            this.Name = Path.GetFileNameWithoutExtension(_string);
            this.Description = ShortenFilePath(_string);
            this.ModPath = _string;
        }

        private string ShortenFilePath(string _string)
        {
            var fname = Path.GetFileName(_string);
            var parentFolder = Path.GetDirectoryName(_string).Split('\\').Last();
            var root = Path.GetPathRoot(_string);
            var res = $@"{root}...\{parentFolder}\{fname}";
            return res;
        }

        public string Name { get => name; set => SetPropertyField("Name", ref name, value); }
        public string Description { get => description; set => SetPropertyField("Description", ref description, value); }
        public bool IsSelected { get => isSelected; set => SetPropertyField("IsSelected", ref isSelected, value); }
        public string ModPath { get => modPath; set => SetPropertyField("ModPath", ref modPath, value); }



        protected void SetPropertyField<T>(string propertyName, ref T field, T newValue)
        {
            if (!EqualityComparer<T>.Default.Equals(field, newValue))
            {
                field = newValue;
                OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
            }
        }
        /// <summary>
        /// Raises the PropertyChanged notification in a thread safe manner
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

    }
}
