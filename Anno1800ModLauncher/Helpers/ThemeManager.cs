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
    public class ThemeWrap 
    {
        public String Path { get; set; }
        public Theme Theme { get; set; } 

        public ThemeWrap() { 
        
        }
    }

    public class ThemeManager : INotifyPropertyChanged
    {
        private string themePath = "Themes";

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

        public static ThemeManager Instance { get; private set; }

        #region Constructors
        public ThemeManager()
        {
            LoadThemes();
            Instance = this;
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region ThemeChanging Members 
        /// <summary>
        /// loads themes from the Themes directory into a collection of ThemeWraps 
        /// </summary>
        public void LoadThemes()
        {
            if (Directory.Exists(themePath))
            {
                //lets do this the stoneage way for better exception handling
                themeList = new ObservableCollection<ThemeWrap>();
                var files = Directory.EnumerateFiles(themePath, "*.json");
                foreach (string file in files) {
                    try
                    {
                        var theme = new ThemeWrap
                        {
                            Theme = JsonConvert.DeserializeObject<Theme>(File.ReadAllText(file, Encoding.UTF8)),
                            //need to delete the themepath here so we just get the filename. maybe there is a better way to do this. 
                            Path = file.Replace(themePath + "\\", "")
                        };
                        if (theme != null)
                            themeList.Add(theme);
                    }
                    catch (JsonSerializationException e)
                    {
                        Console.WriteLine("couldn't deserialize theme: {0}", file);
                    }
                    catch (JsonReaderException e){
                        Console.WriteLine("Error parsing theme: {0}", file);
                    }
                }
            }
        }
        /// <summary>
        /// Changes the theme of the application and automatically stores it as the new default in the application properties
        /// </summary>
        /// <param name="wrap">The themeWrap that the theme should be changed to.</param>
        public void ChangeTheme(ThemeWrap wrap)
        {
            //save themepath in the application properties.
            SaveTheme(wrap.Path);

            var dict = wrap.Theme.toResourceDictionary();
            foreach (var key in dict.Keys)
            {
                Application.Current.Resources[key] = dict[key];
            }
            Console.WriteLine("Changed Theme to {0}", wrap.Theme.ThemeName.English);
        }

        /// <summary>
        /// Saves the theme in the application properties. 
        /// </summary>
        /// <param name="path">the path of the theme</param>
        private void SaveTheme(String path)
        {
            Properties.Settings.Default.Theme = path;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Changes the theme of the application
        /// </summary>
        /// <param name="theme">The filename of the theme file that should be changed to.</param>
        public void ChangeTheme(String theme)
        {
            try
            {
                if (!string.IsNullOrEmpty(theme))
                {
                    ChangeTheme(new ThemeWrap
                    {
                        Theme = JsonConvert.DeserializeObject<Theme>(File.ReadAllText("Themes/" + theme)),
                        Path = "Themes/" + theme
                    });
                }
            }
            catch
            {
                Console.WriteLine("Theme {0}.json couldn't be loaded", theme);
            }
        }
        #endregion 

       
    }
}
