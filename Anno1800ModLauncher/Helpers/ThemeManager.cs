using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Anno1800ModLauncher.Helpers.Serializable;
using Newtonsoft.Json;
using System.IO;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace Anno1800ModLauncher.Helpers
{
    public static class ThemeManager
    {
        private static void ChangeTheme(Theme theme)
        {
            //change the theme to json file format to provide meta info about the themes
            var dict = theme.toResourceDictionary(); 
            foreach (var key in dict.Keys) 
            {
                Application.Current.Resources[key] = dict[key];
                Console.WriteLine("Replacing key {0} with {1}", key, dict[key]);
            }
        }
        public static void SetTheme(Theme theme) {
            ChangeTheme(theme);
        }
        public static void SetTheme(String theme)
        {
            try
            {
                if (!string.IsNullOrEmpty(theme)) {
                    ChangeTheme(JsonConvert.DeserializeObject<Theme>(File.ReadAllText("Themes/" + theme + ".json")));
                    Properties.Settings.Default.Theme = theme;
                }
            }
            catch {
                Console.WriteLine("Theme {0}.json couldn't be loaded", theme);
            }
        }
    }

    public class ThemeLoadingHelper : INotifyPropertyChanged
    {
        private string themePath = "Themes";
        public ThemeLoadingHelper() {
            LoadThemes();
        }

        public void LoadThemes() {
            if (Directory.Exists(themePath)) {
                string path = themePath + "/*.json";
                themeList = new ObservableCollection<Theme>(Directory.EnumerateFiles(themePath, "*.json").
                    Select(
                        //in case json convert get's an error in we are pretty fucked right n
                        

                        d => JsonConvert.DeserializeObject<Theme>(File.ReadAllText(d, Encoding.UTF8))

                    ).Where(w => w != null).ToList());;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<Theme> _themeList; 
        public ObservableCollection<Theme> themeList
        {
            get { return _themeList; }
            set
            {
                _themeList = value;
                OnPropertyChanged("themeList");
            }
        }
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
