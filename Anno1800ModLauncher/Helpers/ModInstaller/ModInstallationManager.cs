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
            Task.Run(async () => await InstallAsync().ConfigureAwait(false)).ContinueWith((e) => InstallationComplete(e.Result));
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

        private void InstallSingle(ZipFile modArchive, string destinationPath)
        {   
            modArchive.ExtractAll(destinationPath, ExtractExistingFileAction.OverwriteSilently);
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
