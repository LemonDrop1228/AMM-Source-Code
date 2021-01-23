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
        private static void ChangeTheme(ThemeWrap wrap)
        {
            //save path of the theme
            Properties.Settings.Default.Theme = wrap.Path;
            Console.WriteLine(wrap.Path);
            Properties.Settings.Default.Save();

            //change the theme to json file format to provide meta info about the themes
            var dict = wrap.Theme.toResourceDictionary(); 
            foreach (var key in dict.Keys) 
            {
                Application.Current.Resources[key] = dict[key];
                Console.WriteLine("Replacing key {0} with {1}", key, dict[key]);
            }
        }
        public static void SetTheme(ThemeWrap theme) {
            ChangeTheme(theme);
        }
        public static void SetTheme(String theme)
        {
            try
            {
                if (!string.IsNullOrEmpty(theme)) {
                    ChangeTheme(new ThemeWrap 
                        { 
                            Theme = JsonConvert.DeserializeObject<Theme>(File.ReadAllText("Themes/" + theme)), Path = "Themes/" + theme 
                        });
                    
                }
            }
            catch {
                Console.WriteLine("Theme {0}.json couldn't be loaded", theme);
            }
        }
    }

    public class ThemeWrap 
    {
        public String Path { get; set; }
        public Theme Theme { get; set; } 

        public ThemeWrap() { 
        
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
                themeList = new ObservableCollection<ThemeWrap>(Directory.EnumerateFiles(themePath, "*.json").
                    Select(
                        d => new ThemeWrap
                        {
                            Theme = JsonConvert.DeserializeObject<Theme>(File.ReadAllText(d, Encoding.UTF8)),
                            //need to delete the themepath here so we just get the filename. maybe there is a better way to do this. 
                            Path = d.Replace(themePath + "\\", "")
                        }
                    ).Where(w => w != null).ToList()) ;;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<ThemeWrap> _themeList; 
        public ObservableCollection<ThemeWrap> themeList
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
