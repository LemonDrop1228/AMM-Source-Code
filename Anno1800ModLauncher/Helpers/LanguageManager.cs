using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Anno1800ModLauncher.Helpers.Enums;

namespace Anno1800ModLauncher.Helpers
{
    /// <summary>
    /// the language manager is a static helper that can set and return the current language easily from anywhere within the application. 
    /// All Strings that have to be localized have to be a dynamic resource. 
    /// Any change to language is automatically saved in the application property "Language".
    /// This is needed for returning localized values from modinfos. 
    /// Currently supports German and English. 
    /// </summary>
    public static class LanguageManager {

        public delegate void LanguageChangedHandler(object source, EventArgs args);

        public static event LanguageChangedHandler LanguageChanged = delegate { };
        public static void OnLanguageChanged() {
            LanguageChanged(null, EventArgs.Empty);
        }
        public static void SetLanguage(HelperEnums.Language lang) {
            //get language file name
            string langCode = ""; 
            switch (lang) {
                case HelperEnums.Language.English: langCode = "english"; break;
                case HelperEnums.Language.German: langCode = "german"; break;
                default:
                    langCode = "english"; break;
            }
            Properties.Settings.Default.Language = (int)lang;
            Properties.Settings.Default.Save(); 

            //replace the current language resource dictionary. 
            var dict = new ResourceDictionary() { Source = new Uri($"Texts/{langCode}.xaml", UriKind.Relative) };
            foreach (var mergeDict in dict.MergedDictionaries)
            {
                Application.Current.Resources.MergedDictionaries.Add(mergeDict);
            }
            foreach (var key in dict.Keys)
            {
                Application.Current.Resources[key] = dict[key];
            }
            OnLanguageChanged();
        }

        public static HelperEnums.Language GetLanguage() {
            int langInt = Properties.Settings.Default.Language;
            return (HelperEnums.Language)langInt;
        }

    }
}
