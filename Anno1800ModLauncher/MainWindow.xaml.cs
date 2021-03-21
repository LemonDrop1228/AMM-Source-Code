
using Anno1800ModLauncher.Helpers.Enums;
using Anno1800ModLauncher.Helpers.ModLoader;
using Anno1800ModLauncher.Extensions;
using MaterialDesignExtensions.Controls;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;
using Anno1800ModLauncher.Helpers;
using System.Runtime.InteropServices;
using static Anno1800ModLauncher.Helpers.Enums.HelperEnums;
using Anno1800ModLauncher.Helpers.SelfUpdater;
using Octokit;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Anno1800ModLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MaterialWindow, INotifyPropertyChanged
    {
        //Test
        private const string WindowTitle = "AMM";
        private const string NewsUrlString = "anno-union.com";
        //private GeckoWebBrowser FoxWebView;
        private Uri currentUri;
        private string GameRootPath { get => Properties.Settings.Default.GameRootPath; }
        private string Moddirectory { get => System.IO.Path.Combine(GameRootPath, Properties.Settings.Default.ModDirectory); }
        private string GamePath { get => System.IO.Path.Combine(GameRootPath, Properties.Settings.Default.GameExecutablePath); }
        private string ModLoaderPath { get => System.IO.Path.Combine(GameRootPath, Properties.Settings.Default.ModLoaderPath); }
        private bool GameFound { get => File.Exists(GamePath); }
        public GameDirectoryWatcher GameRootPathWatcher { get; private set; }
        public bool IsModLoaderBusy { get; private set; }
        private string modLoaderVersion { get => Properties.Settings.Default.CurrentModLoaderVersion; }
        private string nr = Environment.NewLine;
        private ManagerStatus Status = ManagerStatus.bad;
        private bool IsUpdating;

        public LanguageManager LanguageManager = new LanguageManager();

        public SettingsManager SettingsManager { get; set; }

        [DllImport("winmm.dll")]
        public static extern int waveOutSetVolume(IntPtr h, uint dwVolume);

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this; 

            SettingsManager = new SettingsManager();

            //set language to the one that is saved as default in the application properties

            //at first start, set the language to the system language
            //by default, the apps language is -1
            if (Properties.Settings.Default.Language < 0)
            {
                var lang = CultureInfo.InstalledUICulture.Name;
                if (lang.StartsWith("en"))
                {
                    LanguageManager.Instance.SetLanguage(HelperEnums.Language.English);
                }
                else if (lang.StartsWith("de"))
                {
                    LanguageManager.Instance.SetLanguage(HelperEnums.Language.German);
                }
            }
            else {
                LanguageManager.Instance.SetLanguage((HelperEnums.Language)Properties.Settings.Default.Language);
            }
            ThemeManager.Instance.ChangeTheme(Properties.Settings.Default.Theme);
        }

        private void CheckSelfVersion()
        {
            Task.Run(async () => { return await SelfUpdateManager.GetLatestVersionAsset().ConfigureAwait(false); }).ContinueWith((e) => { ContinuationCheck(e.Result); });            
        }

        private void ContinuationCheck(Release release)
        {
            if (release != null)
            {
                var v = SelfUpdateManager.GetReleaseVersion(release);
                if (!SelfUpdateManager.IsLatest(v))
                    PollForUpdate(true, release);
            }
        }

        private void Init()
        {
            Console.SetOut(new ControlWriter(LogRTB, this));
            if (!string.IsNullOrEmpty(GameRootPath))
            {
                GameRootPathWatcher = new GameDirectoryWatcher(GameRootPath);
                GameRootPathWatcher.GameDirectoryUpdated += GameRootPathWatcher_GameDirectoryUpdated;
            }
        }

        private void GameRootPathWatcher_GameDirectoryUpdated()
        {
            this.Dispatcher.BeginInvoke((Action)(() => {
                Console.WriteLine("File changes detected!");
                CheckSettings();
            }));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            waveOutSetVolume(IntPtr.Zero, 0);
            Init();
            Console.WriteLine("Checking status..");

            CheckSelfVersion();
            if (!IsUpdating)
            {
                CheckSettings();
                CheckModLoaderVersion();
            }

        }

        private void ProcessNewGamePath(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                if (GameRootPathWatcher != null)
                {
                    GameRootPathWatcher.Dispose();
                    GameRootPathWatcher = null;
                }
                GameRootPathWatcher = new GameDirectoryWatcher(path);
                GameRootPathWatcher.GameDirectoryUpdated += GameRootPathWatcher_GameDirectoryUpdated;
                Properties.Settings.Default.GameRootPath = path;
                Properties.Settings.Default.Save();
                Properties.Settings.Default.Reload();
                CheckSettings();
                CheckModLoaderVersion();
            }
        }

        private void CheckModLoaderVersion()
        {
            if (GameFound)
            {
                if (!ModLoaderInstaller.IsLatest(modLoaderVersion))
                    PollForUpdate(false); 
            }
        }

        private void PollForUpdate(bool self, Release selfRelease = null)
        {
            switch (self)
            {
                case true:
                    if (AskUserToUpdate($"There is a new version available for Anno 1800 Mod Manager.{nr}Would you like to update now?", "Update Available!"))
                    {
                        IsUpdating = true;
                        UpdateSelf(selfRelease);
                    }
                    break;
                case false:
                    if (AskUserToUpdate($"There is a new version available for Xforces Mod Loader.{nr}Would you like to update now?", "Update Available!"))
                        UpdateModLoader();
                    break;
                default:
                    break;
            }
        }

        private void UpdateModLoader()
        {
            ModLoaderInstaller.GetLatest();
        }

        private void UpdateSelf(Release selfRelease)
        {
            //this.IsEnabled = false;
            SelfUpdateManager.GetLatest(selfRelease);
        }

        private bool AskUserToUpdate(string message, string title)
        {
            return System.Windows.MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }

        private void CheckSettings()
        {
            if (string.IsNullOrEmpty(GameRootPath) || !File.Exists(GamePath))
            {
                HomeView1.SetDetectionState(HelperEnums.DetectionState.None, $"Game cannot be found! Please set the game path to continue...");
            }
            else
            {
                var modDirBool = Directory.Exists(Moddirectory);
                var modLoaderBool = File.Exists(ModLoaderPath);

                if(modDirBool && modLoaderBool)
                {
                    HomeView1.SetDetectionState(HelperEnums.DetectionState.Golden, $"Game detected, mod folder found, and mod loader appears to be installed... Yay!");
                    Status = ManagerStatus.good;
                }
                else if(!modDirBool && !modLoaderBool)
                {
                    HomeView1.SetDetectionState(HelperEnums.DetectionState.GameWithoutModContent, $"Game detected but mod folder needs to be created and mod loader doesn't appear to be installed!");
                    Console.WriteLine("To use mods please create the mod folder and download -> install the mod loader!");
                }
                else if (modDirBool && !modLoaderBool)
                {
                    HomeView1.SetDetectionState(HelperEnums.DetectionState.GameWithModsButNoLoader, $"Game detected and mod folder found but the mod loader is not installed!");
                    Console.WriteLine("To use mods please download and install the mod loader!");
                }
                else if (!modDirBool && modLoaderBool)
                {
                    HomeView1.SetDetectionState(HelperEnums.DetectionState.GameWithLoaderButNoModFolder, $"Game detected and mod loader installed but the mod folder could not be found!");
                    Console.WriteLine("To use mods please create the mod folder!");
                }
            }
            ModListView.LoadList();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Windows.Controls.TabControl tabControl = sender as System.Windows.Controls.TabControl;
            TabItem item = tabControl.SelectedValue as TabItem;
            this.Title = $"{WindowTitle} - {item.Name}";
        }

#region Navigation
        private void Home_Clicked(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 1;
        }
        private void News_Clicked(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 0;
            //NewsView.LoadNews();

        }

        private void Mods_Clicked(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(GameRootPath) && File.Exists(GamePath))
                MainTabControl.SelectedIndex = 2;
            else
                System.Windows.MessageBox.Show("Please set the game path");
        }

        private void Settings_Clicked(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 3;
        }

        private void About_Clicked(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 4;
        }
#endregion

        private void ProcessModDirectoryCreation()
        {
            //CheckSettings();
        }

        private void ProcessModLoaderInstalled(bool status)
        {
            IsModLoaderBusy = status;
            //CheckSettings();
        }

        private void LaunchGame_Clicked(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            try
            {
                Console.WriteLine("Loading Anno 1800... this could take a minute depending on your computer");
                Process.Start(GamePath);
                Console.WriteLine("Playing Anno 1800... ENJOY!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void LogRTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            LogScollViewer.ScrollToBottom();
        }

        private void MainWindow1_Closing(object sender, CancelEventArgs e)
        {
            //HomeView1.DisposeBrowser();
        }

        private void Export_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                var modExportData = ModListView.GetModExportFile();

                string fileNameDynamic = $"{DateTime.Now.ToShortDateString().Replace('/', '.')}_{System.Windows.Forms.Application.ProductVersion}";
                var sf = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = fileNameDynamic,
                    DefaultExt = ".ammxl",
                    Filter = "AMM Mod List (.ammxl)|*.ammxl"
                };

                Nullable<bool> result = sf.ShowDialog();


                if (result == true)
                {
                    string filePath = sf.FileName;
                    ExportModList(filePath, modExportData);
                    Console.WriteLine("Successfully exported your current mod list!");
                    Console.WriteLine($"Mod list can be found here: {filePath}");
                }

                sf = null;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred while trying to export the mod list:");
                Console.WriteLine($"{ex.Message}");
            }
        }

        private void ExportModList(string fileName, string modExportData)
        {
            File.WriteAllText(fileName, modExportData);
            Process.Start("explorer.exe", $@"{fileName}");

        }

        private void Import_Clicked(object sender, RoutedEventArgs e)
        {
            var modList = string.Empty;
            using (var dialog = new System.Windows.Forms.OpenFileDialog()
            {
                Filter = "AMM Mod List (.ammxl)|*.ammxl",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Title = "Select a mod list to load",
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false
            })
            {
                dialog.FileOk += (s, e) => {
                    var d = (s as System.Windows.Forms.OpenFileDialog);
                    if (d != null)
                        modList = d.FileName;
                };
                dialog.ShowDialog();
            }

            if (!string.IsNullOrEmpty(modList))
                ProcessModListImport(modList);
        }

        private void ProcessModListImport(string modList)
        {
            var modData = File.ReadAllText(modList);
            if (!string.IsNullOrEmpty(modData)) {
                try
                {
                    var modJson = modData.Base64Decode();
                    var modListObj = JsonConvert.DeserializeObject<ObservableCollection<ModModel>>(modJson);
                    ModListView.UpdateByImport(modListObj);
                    Console.WriteLine($"Successfully imported mod list: {modList}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error occurred while trying to import the mod list: {modList}");
                    Console.WriteLine($"{ex.Message}");
                }
            }
        }


        #region INotifyPropertyChanged Members

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
    }
}
