using Anno1800ModLauncher.Helpers.Enums;
using Anno1800ModLauncher.Helpers.ModLoader;
using MaterialDesignThemes.Wpf;
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
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CefSharp.Wpf;
using CefSharp;
using Anno1800ModLauncher.CustomDialogs;
using Anno1800ModLauncher.Helpers.ModInstaller;

namespace Anno1800ModLauncher.Views
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    public partial class HomeView : System.Windows.Controls.UserControl
    {
        public delegate void GamePathSelectedEventHandler(string path);
        [Browsable(true)]
        public event GamePathSelectedEventHandler GamePathSelectedEvent;

        public delegate void ModDirectoryCreatedEventHandler();
        [Browsable(true)]
        public event ModDirectoryCreatedEventHandler ModDirectoryCreatedEvent;

        public delegate void ModLoaderInstalledEventHandler(bool status);
        [Browsable(true)]
        public event ModLoaderInstalledEventHandler ModLoaderInstalledEvent;

        [Obsolete]
        public HomeView()
        {
            InitializeComponent();

            //webBrowser = new ChromiumWebBrowser();
            //webBrowser.Loaded += W_Loaded;
            //BrowserPanel.Children.Add(webBrowser);
        }

        //private void W_Loaded(object sender, RoutedEventArgs e)
        //{
        //    if(!DesignerProperties.GetIsInDesignMode(this))
        //        (sender as ChromiumWebBrowser).Load(newsUrl);
        //}

        internal void SetDetectionState(HelperEnums.DetectionState state, string message = null)
        {
            if (!string.IsNullOrEmpty(message))
                Console.WriteLine(message);

            switch (state)
            {
                case HelperEnums.DetectionState.None:
                    #region Requires complete configuration
                    GameStatus.Background = new SolidColorBrush(Colors.Salmon);
                    GameStatus.Content = new MaterialDesignThemes.Wpf.PackIcon() { Kind = MaterialDesignThemes.Wpf.PackIconKind.EmoticonSad, Width = 24, Height = 24 };

                    ModDirStatus.Background = new SolidColorBrush(Colors.Salmon);
                    MidDirText.Text = "Create Mod Folder";
                    ModDirStatus.Content = new MaterialDesignThemes.Wpf.PackIcon() { Kind = MaterialDesignThemes.Wpf.PackIconKind.EmoticonSad, Width = 24, Height = 24 };

                    ModLoaderStatus.Background = new SolidColorBrush(Colors.Salmon);
                    ModLoaderStatus.Content = new MaterialDesignThemes.Wpf.PackIcon() { Kind = MaterialDesignThemes.Wpf.PackIconKind.EmoticonSad, Width = 24, Height = 24 };
                    #endregion
                    break;
                case HelperEnums.DetectionState.GameWithoutModContent:
                    #region Requires mod directory and mod loader
                    GameStatus.Background = new SolidColorBrush(Colors.Green);
                    GameStatus.Content = new MaterialDesignThemes.Wpf.PackIcon() { Kind = MaterialDesignThemes.Wpf.PackIconKind.Check, Width = 24, Height = 24 };

                    ModDirStatus.Background = new SolidColorBrush(Colors.Salmon);
                    MidDirText.Text = "Create Mod Folder";
                    ModDirStatus.Content = new MaterialDesignThemes.Wpf.PackIcon() { Kind = MaterialDesignThemes.Wpf.PackIconKind.EmoticonSad, Width = 24, Height = 24 };

                    ModLoaderStatus.Background = new SolidColorBrush(Colors.Salmon);
                    ModLoaderStatus.Content = new MaterialDesignThemes.Wpf.PackIcon() { Kind = MaterialDesignThemes.Wpf.PackIconKind.EmoticonSad, Width = 24, Height = 24 };
                    #endregion
                    break;
                case HelperEnums.DetectionState.GameWithModsButNoLoader:
                    #region Requires mod loader
                    GameStatus.Background = new SolidColorBrush(Colors.Green);
                    GameStatus.Content = new MaterialDesignThemes.Wpf.PackIcon() { Kind = MaterialDesignThemes.Wpf.PackIconKind.Check, Width = 24, Height = 24 };

                    ModDirStatus.Background = new SolidColorBrush(Colors.Green);
                    MidDirText.Text = "Open Mod Directory";
                    ModDirStatus.Content = new MaterialDesignThemes.Wpf.PackIcon() { Kind = MaterialDesignThemes.Wpf.PackIconKind.Check, Width = 24, Height = 24 };

                    ModLoaderStatus.Background = new SolidColorBrush(Colors.Salmon);
                    ModLoaderStatus.Content = new MaterialDesignThemes.Wpf.PackIcon() { Kind = MaterialDesignThemes.Wpf.PackIconKind.EmoticonSad, Width = 24, Height = 24 };
                    #endregion
                    break;
                case HelperEnums.DetectionState.GameWithLoaderButNoModFolder:
                    #region Requires mod folder creation
                    GameStatus.Background = new SolidColorBrush(Colors.Green);
                    GameStatus.Content = new MaterialDesignThemes.Wpf.PackIcon() { Kind = MaterialDesignThemes.Wpf.PackIconKind.Check, Width = 24, Height = 24 };

                    ModDirStatus.Background = new SolidColorBrush(Colors.Salmon);
                    MidDirText.Text = "Create Mod Folder";
                    ModDirStatus.Content = new MaterialDesignThemes.Wpf.PackIcon() { Kind = MaterialDesignThemes.Wpf.PackIconKind.EmoticonSad, Width = 24, Height = 24 };

                    ModLoaderStatus.Background = new SolidColorBrush(Colors.Green);
                    LoaderButtonText.Text = "Re-Install Mod Loader";
                    ModLoaderStatus.Content = new MaterialDesignThemes.Wpf.PackIcon() { Kind = MaterialDesignThemes.Wpf.PackIconKind.Check, Width = 24, Height = 24 };
                    #endregion
                    break;
                case HelperEnums.DetectionState.GameWithOldLoaderButNoModFolder:
                    #region Requires mod folder creation
                    GameStatus.Background = new SolidColorBrush(Colors.Green);
                    GameStatus.Content = new MaterialDesignThemes.Wpf.PackIcon() { Kind = MaterialDesignThemes.Wpf.PackIconKind.Check, Width = 24, Height = 24 };

                    ModDirStatus.Background = new SolidColorBrush(Colors.Salmon);
                    MidDirText.Text = "Create Mod Folder";
                    ModDirStatus.Content = new MaterialDesignThemes.Wpf.PackIcon() { Kind = MaterialDesignThemes.Wpf.PackIconKind.EmoticonSad, Width = 24, Height = 24 };

                    ModLoaderStatus.Background = new SolidColorBrush(Colors.Yellow);
                    ModLoaderStatus.Content = new MaterialDesignThemes.Wpf.PackIcon() { Kind = MaterialDesignThemes.Wpf.PackIconKind.Exclamation, Width = 24, Height = 24 };
                    #endregion
                    break;
                case HelperEnums.DetectionState.Golden:
                    #region Configurtion is ready
                    GameStatus.Background = new SolidColorBrush(Colors.Green);
                    GameStatus.Content = new MaterialDesignThemes.Wpf.PackIcon() { Kind = MaterialDesignThemes.Wpf.PackIconKind.Check, Width = 24, Height = 24 };

                    ModDirStatus.Background = new SolidColorBrush(Colors.Green);
                    MidDirText.Text = "Open Mod Directory";
                    ModDirStatus.Content = new MaterialDesignThemes.Wpf.PackIcon() { Kind = MaterialDesignThemes.Wpf.PackIconKind.Check, Width = 24, Height = 24 };

                    ModLoaderStatus.Background = new SolidColorBrush(Colors.Green);
                    LoaderButtonText.Text = "Re-Install Mod Loader";
                    ModLoaderStatus.Content = new MaterialDesignThemes.Wpf.PackIcon() { Kind = MaterialDesignThemes.Wpf.PackIconKind.Check, Width = 24, Height = 24 };
                    #endregion
                    break;
                case HelperEnums.DetectionState.GoldenButOld:
                    #region Configurtion is ready
                    GameStatus.Background = new SolidColorBrush(Colors.Green);
                    GameStatus.Content = new MaterialDesignThemes.Wpf.PackIcon() { Kind = MaterialDesignThemes.Wpf.PackIconKind.Check, Width = 24, Height = 24 };

                    ModDirStatus.Background = new SolidColorBrush(Colors.Green);
                    MidDirText.Text = "Open Mod Directory";
                    ModDirStatus.Content = new MaterialDesignThemes.Wpf.PackIcon() { Kind = MaterialDesignThemes.Wpf.PackIconKind.Check, Width = 24, Height = 24 };

                    ModLoaderStatus.Background = new SolidColorBrush(Colors.Yellow);
                    LoaderButtonText.Text = "Re-Install Mod Loader";
                    ModLoaderStatus.Content = new MaterialDesignThemes.Wpf.PackIcon() { Kind = MaterialDesignThemes.Wpf.PackIconKind.Exclamation, Width = 24, Height = 24 };
                    #endregion
                    break;
                case HelperEnums.DetectionState.InstallingModLoader:
                    ModLoaderStatus.Background = new SolidColorBrush(Colors.SkyBlue);
                    ModLoaderStatus.Content = new MaterialDesignThemes.Wpf.PackIcon() { Kind = MaterialDesignThemes.Wpf.PackIconKind.CloudDownload, Width = 24, Height = 24 };
                    break;
                default:
                    break;
            }
        }


        private void CreateModDirectory(object sender, RoutedEventArgs e)
        {
            string gamePath = System.IO.Path.Combine(Properties.Settings.Default.GameRootPath, Properties.Settings.Default.GameExecutablePath);
            string gameRootPath = Properties.Settings.Default.GameRootPath;
            string modDirPath = System.IO.Path.Combine(Properties.Settings.Default.GameRootPath, Properties.Settings.Default.ModDirectory);


            if (string.IsNullOrEmpty(gameRootPath)
                || !File.Exists(gamePath))
            {
                Console.WriteLine("Whoa! - The game directory hasn't been correctly set yet!");
            }
            else if (Directory.Exists(modDirPath))
            {
                Console.WriteLine("Opening mod directory...");
                Process.Start(modDirPath);
            }
            else
            {
                try
                {
                    string path = System.IO.Path.Combine(Properties.Settings.Default.GameRootPath, Properties.Settings.Default.ModDirectory);
                    Directory.CreateDirectory(path);
                    var di = new DirectoryInfo(path);
                    di.Attributes = ~FileAttributes.ReadOnly;
                    ModDirectoryCreatedEvent();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private string SetGameRoot()
        {
            var path = "";
            var dialog = new VistaFolderBrowserDialog();
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                path = dialog.SelectedPath;
            }
            return path;
        }

        private void GameButton_Click(object sender, RoutedEventArgs e)
        {
            GamePathSelectedEvent(SetGameRoot());
        }

        private void DownloadInstallModLoader(object sender, RoutedEventArgs e)
        {
            string gamePath = System.IO.Path.Combine(Properties.Settings.Default.GameRootPath, Properties.Settings.Default.GameExecutablePath);
            string gameRootPath = Properties.Settings.Default.GameRootPath;
            if (string.IsNullOrEmpty(gameRootPath)
                || !File.Exists(gamePath))
            {
                Console.WriteLine("Whoa! - The game directory hasn't been correctly set yet!");
            }
            else
            {
                var installed = true;
                ModLoaderInstalledEvent(true);
                ModLoaderInstaller.InstallModLoader();
                ModLoaderInstalledEvent(false);
            }

        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/xforce/anno1800-mod-loader/");
        }

        private void InstallZip_Click(object sender, RoutedEventArgs e)
        {

        }

        private void InstallNexus_Click(object sender, RoutedEventArgs e)
        {

        }

        private void InstallRepo_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SearchZip_Click(object sender, RoutedEventArgs e)
        {
            string[] fileList = null;

            using (var dialog = new OpenFileDialog() { 
                Filter = "Archive Files|*.zip;*.7z;*.rar",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Title = "Select a mod archive to install",
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = true       
            })
            {
                dialog.FileOk += (s,e) => {
                    var d = (s as OpenFileDialog);
                    if(d != null)
                        fileList = d.FileNames;
                };
                dialog.ShowDialog();
            }

            if (fileList != null)
                ProcessInstallationRequest(fileList);
        }

        private void ProcessInstallationRequest(string[] fileNames)
        {
            if(fileNames.Count() > 0)
            {
                var installDialog = new ModInstallationDialog(new ModInstallationManager(fileNames));
                if(installDialog.ShowModDialog() == MessageBoxResult.OK)
                {
                    Console.WriteLine("Installed mods successfully!");
                }
                else
                {
                    Console.WriteLine("There was an issue installing your mods!");
                }
                
            }
        }

        private string GetModMessageForInstallation(string[] fileNames)
        {
            var nr = Environment.NewLine;
            var templateMsg = $"Installing the following files:{nr}{{0}}{nr}{nr}Would you like to proceed?";
            var modListString = string.Join($"{nr}", GetFormattedPaths(fileNames));
            return string.Format(templateMsg, modListString);
        }

        private string[] GetFormattedPaths(string[] fileNames)
        {
            for (int i = 0; i < fileNames.Count(); i++)
            {
                fileNames[i] = $" - {fileNames[i]}";
            }

            return fileNames;
        }
    }
}
