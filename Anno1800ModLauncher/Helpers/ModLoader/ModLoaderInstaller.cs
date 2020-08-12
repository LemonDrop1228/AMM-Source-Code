using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Octokit;
using Ionic.Zip;
using Ionic.Zlib;
using System.Threading;

namespace Anno1800ModLauncher.Helpers.ModLoader
{
    public static class ModLoaderInstaller
    {
        private static GitHubClient client = new GitHubClient(new ProductHeaderValue("anno-mod-launcher"));
        private static string LocalModLoaderDir = $@"{Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)}\Libs";
        private static string modLoaderRoot = Path.Combine(Properties.Settings.Default.GameRootPath, Properties.Settings.Default.ModLoaderRoot);

        public static bool IsBusy { get; private set; }

        public static bool InstallModLoader()
        {
            var isInstalled = false;
            IsBusy = true;
            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
            try
            {
                InstallFromLocal();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error installing Mod Loader: {ex.InnerException}");
            }
            finally
            {
                IsBusy = false;
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
            }
            return isInstalled;

        }

        private static void InstallFromLocal()
        {
            string sourceFileName1 = $@"{LocalModLoaderDir}\python35.dll";
            string destFileName1 = $@"{modLoaderRoot}python35.dll";
            string sourceFileName2 = $@"{LocalModLoaderDir}\python35_ubi.dll";
            string destFileName2 = $@"{modLoaderRoot}python35_ubi.dll";

            try
            {
                byte[] main = File.ReadAllBytes(sourceFileName1);
                MakeEditable(destFileName1);
                File.WriteAllBytes(destFileName1, main);
                main = null;
                byte[] loader = File.ReadAllBytes(sourceFileName2);
                File.WriteAllBytes(destFileName2, loader);
                loader = null;
            }
            catch (Exception ex)
            {
                throw;
            }

            Console.WriteLine($"Successfully installed Mod Loader to: {modLoaderRoot}");
        }

        private static void MakeEditable(string destFileName1)
        {
            var fi = new FileInfo(destFileName1);
            fi.IsReadOnly = false;
            fi = null;
        }

        public static void GetLatest()
        {
            var release = client.Repository.Release.GetLatest("xforce", "anno1800-mod-loader").Result;
            if (release != null && release.Assets.Any(i => i.Name.Contains("loader.zip")))
            {
                Console.WriteLine($"Found the latest Mod Loader: {release.Name}");
                var downloadURL = release.Assets.FirstOrDefault(i => i.Name.Contains("loader.zip")).BrowserDownloadUrl;
                string fileName = Path.Combine(LocalModLoaderDir, "loader.zip");
                string modLoaderPath = Path.Combine(Properties.Settings.Default.GameRootPath, Properties.Settings.Default.ModLoaderPath);

                using (var client = new WebClient())
                {
                    client.DownloadDataCompleted += (s, e) => { File.WriteAllBytes(fileName, e.Result); UnpackZip(fileName); InstallFromLocal();
                        Console.WriteLine($"Updated the local Mod Loader version to: {fileName}");
                        Properties.Settings.Default.CurrentModLoaderVersion = release.Name;
                        Properties.Settings.Default.Save();
                        Properties.Settings.Default.Reload();
                        release = null; };
                    client.DownloadProgressChanged += (s, e) => { Console.Write($"Downloading Mod Loader: {e.ProgressPercentage}%"); };
                    client.DownloadDataAsync(new Uri(downloadURL));
                }
            }
        }

        private static void UnpackZip(string fileName)
        {
            using (ZipFile zip = ZipFile.Read(fileName))
            {
                zip.TempFileFolder = Path.GetTempPath();
                zip.ExtractAll(LocalModLoaderDir, ExtractExistingFileAction.OverwriteSilently);
            }
            string sourceFileName1 = $@"{LocalModLoaderDir}\python35.dll";
            MakeEditable(sourceFileName1);
        }

        internal static bool IsLatest(string currentModLoaderVersion)
        {
            return GetLatestVersionId() == currentModLoaderVersion;
        }

        internal static string GetLatestVersionId()
        {
            string name = string.Empty;
            try
            {
                Release result = client.Repository.Release.GetLatest("xforce", "anno1800-mod-loader").Result;
                name = result.Name;
                result = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException);
            }
            return name;
        }
    }
}
