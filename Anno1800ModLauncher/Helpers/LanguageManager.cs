using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Anno1800ModLauncher.Helpers.Enums;

namespace Anno1800ModLauncher.Helpers
{
    /// <summary>
    /// the language manager is a static helper that can set and return the current language easily from anywhere within the application. 
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
            string langCode = ""; 
            switch (lang) {
                case HelperEnums.Language.English: langCode = "en-EN"; break;
                case HelperEnums.Language.German: langCode = "de-DE"; break;
                default:
                    langCode = "en-EN"; break;
            }
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(langCode);
            Properties.Settings.Default.Language = (int)lang;
            OnLanguageChanged();
        }

        public static HelperEnums.Language GetLanguage() {
            string langCode = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
            HelperEnums.Language lang = 0;
            switch(langCode){
                case "en-EN": lang = HelperEnums.Language.English; break;
                case "de-DE": lang = HelperEnums.Language.German; break;
                default: lang = HelperEnums.Language.English; break; 

            }
            return lang;
        }
    }
}
