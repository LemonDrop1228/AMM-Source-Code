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
    /// the language manager is a helper that can set and return the current language easily from anywhere within the application by accessing the static LanguageManager.Instance
    /// All Strings that have to be localized have to be a dynamic resource. 
    /// Any change to language is automatically saved in the application property "Language".
    /// This is needed for returning localized values from modinfos. 
    /// Currently supports German and English. 
    /// </summary>
    public class LanguageManager {


        #region Events
        public delegate void LanguageChangedHandler(object source, EventArgs args);

        public event LanguageChangedHandler LanguageChanged = delegate { };

        #endregion

        public static LanguageManager Instance;

        #region Constructors
        public LanguageManager()
        {
            Instance = this;
        }
        #endregion

        #region LanguageModifying Members
        public void SetLanguage(HelperEnums.Language lang) {
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

            //log language change to english. 
            Console.WriteLine("Changed Language to: {0}", lang);

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

        public HelperEnums.Language GetLanguage() {
            int langInt = Properties.Settings.Default.Language;
            return (HelperEnums.Language)langInt;
        }
        #endregion

        public void OnLanguageChanged()
        {
            LanguageChanged(null, EventArgs.Empty);
        }
    }
}
