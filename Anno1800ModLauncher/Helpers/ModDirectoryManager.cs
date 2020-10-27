using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Anno1800ModLauncher.Extensions;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading;
using Ookii.Dialogs.Wpf;
using System.Net;
using Ionic.Zip;
using Ionic.Zlib;
using System.Windows;

namespace Anno1800ModLauncher.Helpers
{
    public class ModDirectoryManager : INotifyPropertyChanged
    {
        private ObservableCollection<ModModel> _baseData;

        private ObservableCollection<ModModel> _modList;
        public ObservableCollection<ModModel> modList { get { return _modList; } set {
                _modList = value;
                OnPropertyChanged("modList");
            } }
        private string modPath { get => Path.Combine(Properties.Settings.Default.GameRootPath, Properties.Settings.Default.ModDirectory); }
        public Ookii.Dialogs.Wpf.ProgressDialog currentProgDiag { get; private set; }

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

        public static ModDirectoryManager Instance { get; private set; } = null;

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public ModDirectoryManager()
        {
            Instance = Instance ?? this;
            LoadMods();
        }

        public void LoadMods()
        {
            if (Directory.Exists(modPath))
            {
                _baseData = modList = new ObservableCollection<ModModel>(Directory.EnumerateDirectories(modPath).
                        Select(d => new ModModel
                        {
                            Path = d,
                            Name = Path.GetFileName(d).TrimDash(),
                            IsActive = Path.GetFileName(d).IsActive(),
                            Icon = (Path.GetFileName(d).IsActive()) ? "CheckBold" : "NoEntry",
                            Color = (Path.GetFileName(d).IsActive()) ? "DarkGreen" : "Red"
                        }).Where(w => w.Name != ".cache").ToList().OrderByDescending(s => s.IsActive));
                if (modList.Count > 0)
                    Console.WriteLine($"Found {modList.Count} mods! Active: {modList.Count(i => i.IsActive)} / Inactive: {modList.Count(i => !i.IsActive)}");
                else
                    Console.WriteLine("Found no mods! You should check out NexusMods for some sweet mods...");
            }
            else
                modList = null;
        }

        internal bool ActivateMod(ModModel i)
        {
            string v = Path.GetDirectoryName(i.Path) + @"\";
            try
            {
                string destDirName = $"{v}{i.Name}";
                Directory.Move(i.Path, destDirName);
                i.Path = destDirName;
                Console.WriteLine($"Activated - {i.Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }

        internal bool DeactivateMod(ModModel i)
        {
            string v = Path.GetDirectoryName(i.Path) + @"\";
            try
            {
                string destDirName = $@"{v}-{i.Name}";
                File.SetAttributes(i.Path, FileAttributes.Normal);
                Directory.Move(i.Path, destDirName);
                i.Path = destDirName;
                Console.WriteLine($"De-Activated - {i.Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }

        internal string GetReadMeText(ModModel i)
        {
            var res = string.Empty;
            string contentPath = $@"{i.Path}\content_en.txt";
            string readmePath = $@"{i.Path}\readme.txt";
            if (File.Exists(contentPath))
                res = File.ReadAllText(contentPath);
            if (File.Exists(readmePath))
                res += Environment.NewLine + File.ReadAllText(readmePath);
            return res;
        }

        internal ImageSource GetModBanner(ModModel i)
        {
            ImageSource res = null;
            string contentPath = $@"{i.Path}\banner.png";
            if (File.Exists(contentPath))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = new MemoryStream(File.ReadAllBytes(contentPath));
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                res = bitmap;
                bitmap.StreamSource.Dispose();
                bitmap = null;
            }
            return res;
        }

        internal ObservableCollection<ModModel> FilterMods(string filterText, bool? filterStatus)
        {
            if (string.IsNullOrEmpty(filterText) && filterStatus == null)
                return _baseData;
            else if (string.IsNullOrEmpty(filterText) && filterStatus != null)
            {
                return _baseData.Where(o => o.IsActive == filterStatus).ToObservableCollection();
            }
            else if (!string.IsNullOrEmpty(filterText) && filterStatus == null)
            {
                return _baseData.Where(o => o.Name.ToLower().Contains(filterText.ToLower())).ToObservableCollection();
            }
            else 
            {
                return _baseData.Where(o => o.Name.ToLower().Contains(filterText.ToLower()) && o.IsActive == filterStatus).ToObservableCollection();
            }
        }

        internal void DownloadInstallNewMod(string v)
        {
            currentProgDiag = new ProgressDialog();
            currentProgDiag.Description = v.Split('/').Last().Split('?').First().Replace("%20", " ");
            currentProgDiag.Text = "Downloading and Installing...";
            currentProgDiag.WindowTitle = "Anno 1800 Mod Manager - Mod Downloader";
            currentProgDiag.ProgressBarStyle = ProgressBarStyle.MarqueeProgressBar;
            currentProgDiag.Show();
            this.GetNewMod(new Uri(v));            
        }

        private void GetNewMod(Uri uri)
        {
            using (var w = new WebClient())
            {

                W_DownloadDataCompleted(w.DownloadData(uri));
            }
        }

        private void W_DownloadDataCompleted(byte[] data)
        {
            string downloadPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\Downloads\test.zip";
            File.WriteAllBytes(downloadPath, data);
            using (var z = new ZipFile(downloadPath))
            {
                z.TempFileFolder = Path.GetTempPath();
                z.ExtractExistingFile = ExtractExistingFileAction.InvokeExtractProgressEvent;
                z.ExtractProgress += Z_ExtractProgress;
                z.ExtractAll(modPath);
                File.Delete(downloadPath);
            }

            if (currentProgDiag != null)
            {
                currentProgDiag.Dispose();
                currentProgDiag = null;
            }
        }

        private void Z_ExtractProgress(object sender, ExtractProgressEventArgs e)
        {
            if(e.EventType == ZipProgressEventType.Extracting_ExtractEntryWouldOverwrite)
            {
                using (TaskDialog dialog = new TaskDialog())
                {
                    dialog.WindowTitle = "Mod Installation";
                    dialog.MainInstruction = "Mod already exists.";
                    dialog.Content = "You have previously installed this mod, continuing will overwrite the current version.";
                    dialog.Footer = "Click 'Yes' to overwrite the current version or 'No' to cancel installation.";
                    dialog.FooterIcon = TaskDialogIcon.Warning;
                    TaskDialogButton okButton = new TaskDialogButton(ButtonType.Yes);
                    TaskDialogButton cancelButton = new TaskDialogButton(ButtonType.No);
                    dialog.Buttons.Add(okButton);
                    dialog.Buttons.Add(cancelButton);
                    if (dialog.ShowDialog() != okButton)
                        e.Cancel = true;
                }
            }
        }
    }

    public class ModModel : INotifyPropertyChanged
    {
        private string name;
        private string path;
        private bool isActive;
        private string icon;
        private string color;
        private bool isSelected;

        public string Name { get => name; set => SetPropertyField("Name",ref name,value); }
        public string Path { get => path; set => SetPropertyField("Path", ref path, value); }
        public bool IsActive { get => isActive; set => SetPropertyField("IsActive", ref isActive, value); }
        public string Icon { get => icon; set => SetPropertyField("Icon", ref icon, value); }
        public string Color { get => color; set => SetPropertyField("Color", ref color, value); }
        public bool IsSelected { get => isSelected; set => SetPropertyField("IsSelected", ref isSelected, value); }

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


        public override string ToString()
        {
            return Name;
        }
    }
}
