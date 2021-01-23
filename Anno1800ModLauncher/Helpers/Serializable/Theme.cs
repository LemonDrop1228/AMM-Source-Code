using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SerializableModinfo;

namespace Anno1800ModLauncher.Helpers.Serializable
{
    public enum ResourceType { ColorBrush, Image }
    public class Theme : INotifyPropertyChanged
    {
        [NonSerialized] private String _ShowName;
        public String ShowName {
            get { return _ShowName; }
            set {
                _ShowName = value;
                OnPropertyChanged("ShowName");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        
        public Theme() {
            LanguageManager.LanguageChanged += UpdateLanguage;
        }

        public void UpdateLanguage(object sender, EventArgs e) {
            ShowName = ThemeName.getText(); 
        }
        
        public Localized ThemeName { get; set; }
        public String GradientColorLight { get; set; }
        public String GradientColorDark { get; set; }
        public KeyValue[] Keys { get; set; }

        /// <summary>
        /// </summary>
        /// <returns>a resource dictionary of all keys with their respective values.</returns>
        public ResourceDictionary toResourceDictionary() {
            var res = new ResourceDictionary();
            foreach (KeyValue Key in Keys) {
                Console.WriteLine("Key {0} marked as {1}", Key.Key, Key.ReadAs);
                switch ((ResourceType)Key.ReadAs) {
                    case ResourceType.ColorBrush:
                        SolidColorBrush brush = new SolidColorBrush();
                        try
                        {
                            Color c = (Color)ColorConverter.ConvertFromString(Key.Value);
                            brush.Color = c; 
                            res.Add(Key.Key, brush);
                            Console.WriteLine("Theme -> adding Color: " + Key.Key);
                        }
                        catch { }
                        break;
                    case ResourceType.Image:
                        try
                        {
                            ImageSource image = new BitmapImage(new Uri(Key.Value));
                            res.Add(Key.Key, image);
                            Console.WriteLine("Theme -> adding Image: " + Key.Key);
                        }
                        catch { }                        
                        break;
                    default: break; 
                        
                }
                
            }
            return res; 
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

    public class KeyValue{
        public String Key;
        public String Value;
        public ResourceType ReadAs;
    }
}
