using Anno1800ModLauncher.Helpers;
using Anno1800ModLauncher.Helpers.Enums;
using Anno1800ModLauncher.Helpers.Serializable;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Anno1800ModLauncher.Views
{
    /// <summary>
    /// Interaktionslogik für SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl, INotifyPropertyChanged
    {
        private ThemeManager _ThemeManager;
        public ThemeManager ThemeManager
        {
            get { return _ThemeManager; }
            set
            {
                _ThemeManager = value;
                OnPropertyChanged("ThemeManager");
            }
        }

        private SettingsManager _SettingsManager;
        public SettingsManager SettingsManager
        {
            get { return _SettingsManager; }
            set
            {
                _SettingsManager = value;
                OnPropertyChanged("SettingsManager");
            }
        }

        public SettingsView()
        {
            InitializeComponent();
            ThemeManager = new ThemeManager();
            SettingsManager = new SettingsManager(); 
            this.DataContext = this;
            LanguageComboBox.SelectedIndex = (int)LanguageManager.Instance.GetLanguage();
        }
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void ThemeSelection_SelectionChanged(object sender, RoutedEventArgs e)
        {
            ThemeWrap theme = (ThemeWrap)ListViewThemes.SelectedItems[0];
            ThemeManager.Instance.ChangeTheme(theme);
        }

        private void ConsoleOutput_Toggled(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Show Console Output: {0}", SettingsManager.Instance.Visibility);
            Properties.Settings.Default.ConsoleVisibility = SettingsManager.Instance.Visibility; 
        }

        public void LanguageSelection_LanguageChanged(object sender, RoutedEventArgs e) {
            if (LanguageComboBox.SelectedValue != null)
            {
                if (LanguageComboBox.SelectedValue.Equals(LanguageComboBoxItemEnglish))
                {
                    LanguageManager.Instance.SetLanguage(HelperEnums.Language.English);
                }
                else if (LanguageComboBox.SelectedValue.Equals(LanguageComboBoxItemGerman))
                {
                    LanguageManager.Instance.SetLanguage(HelperEnums.Language.German);
                }
            }
            //disallow user to clear the combo box ^^
            else {
                if (LanguageManager.Instance.GetLanguage() == HelperEnums.Language.English)
                {
                    LanguageComboBox.SelectedValue =(LanguageComboBoxItemEnglish);
                }
                else if (LanguageManager.Instance.GetLanguage() == HelperEnums.Language.German)
                {
                    LanguageComboBox.SelectedValue = (LanguageComboBoxItemGerman);
                }
            }
                
        }
    }
}
