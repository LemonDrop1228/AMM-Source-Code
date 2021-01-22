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
        private ThemeLoadingHelper _themeLoadingHelper;
        public ThemeLoadingHelper themeLoadingHelper
        {
            get { return _themeLoadingHelper; }
            set
            {
                _themeLoadingHelper = value;
                OnPropertyChanged("themeLoadingHelper");
            }
        }
        public SettingsView()
        {
            InitializeComponent();
            themeLoadingHelper = new ThemeLoadingHelper();
            this.DataContext = this;
            LanguageComboBox.SelectedIndex = (int)LanguageManager.GetLanguage();
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
            Console.WriteLine("Changed Theme");
            Theme theme = (Theme)ListViewThemes.SelectedItems[0];
            ThemeManager.SetTheme(theme);
        }

        private void LanguageSelection_LanguageChanged(object sender, RoutedEventArgs e) {
            if (LanguageComboBox.SelectedValue.Equals(LanguageComboBoxItemEnglish)) {
                LanguageManager.SetLanguage(HelperEnums.Language.English);
            }
            else if (LanguageComboBox.SelectedValue.Equals(LanguageComboBoxItemGerman))
            {
                LanguageManager.SetLanguage(HelperEnums.Language.German);
            }
        }
    }
}
