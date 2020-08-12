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
using System.Diagnostics;

namespace Anno1800ModLauncher.Helpers.SelfUpdater
{
    public static class SelfUpdateManager
    {
        private static GitHubClient client = new GitHubClient(new ProductHeaderValue("anno-mod-launcher"));
        private static string currentVersion = System.Windows.Forms.Application.ProductVersion;

        public static void GetLatest(Release release)
        {
            if (release != null)
            {
                Console.WriteLine("UPDATING MOD MANAGER!  ---  PLEASE WAIT FOR THE UPDATE TO COMPLETE!");
                var downloadURL = release.Assets.First().BrowserDownloadUrl;
                string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", release.Assets.First().Name);

                using (var client = new WebClient())
                {
                    client.DownloadDataCompleted += (s, e) =>
                    {
                        File.WriteAllBytes(fileName, e.Result);
                        Process.Start(fileName);
                        release = null;
                        Environment.Exit(0);
                    };
                    client.DownloadProgressChanged += (s, e) => { Console.Write($"Downloading version {release.TagName}: {e.ProgressPercentage}%"); };
                    client.DownloadDataAsync(new Uri(downloadURL));
                }
            }
        }

        public static string GetReleaseVersion(Release release)
        {
            return release.TagName.Replace("v", "").Replace("-beta", "");
        }

        public static bool IsLatest(string release)
        {
            var v1 = new Version(release);
            var v2 = new Version(currentVersion);
            var results = v1 <= v2;
            return results;
        }

        public static async Task<Release> GetLatestVersionAsset()
        {
            Release res = null;
            try
            {
                Release result = await client.Repository.Release.GetLatest("LemonDrop1228", "anno1800-mod-manager").ConfigureAwait(false);
                res = result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException);
                return res;
            }
            return res;
        }
    }
}
