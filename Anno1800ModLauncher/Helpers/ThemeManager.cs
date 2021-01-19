using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anno1800ModLauncher.Helpers
{
    public static class ThemeManager
    {
        
        private static void ChangeTheme(Uri uri)
        {
            var dict = new ResourceDictionary() { Source = uri };
            foreach (var mergeDict in dict.MergedDictionaries) 
            {
                Application.Current.Resources.MergedDictionaries.Add(mergeDict);
            }
            foreach (var key in dict.Keys) {
                Application.Current.Resources[key] = dict[key];
            }
        }
        public static void SetTheme(String theme)
        {
            try
            {
                if (!string.IsNullOrEmpty(theme))
                    ChangeTheme(new Uri($"Themes/{theme}.xaml", UriKind.Relative));
            }
            catch { }
        }
    }
}
