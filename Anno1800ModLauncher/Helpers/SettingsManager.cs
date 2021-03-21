using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Anno1800ModLauncher.Helpers
{
    public class SettingsManager : INotifyPropertyChanged
    {
        public static SettingsManager Instance { get; set; }

        #region Constructors
        public SettingsManager()
        {
            
            Visibility = Properties.Settings.Default.ConsoleVisibility;
            Instance = this;
        }
        #endregion

        #region SettingVariables
        private bool _visibility;
        public bool Visibility
        {
            get
            {
                return _visibility;
            }
            set
            {
                _visibility = value;
                OnPropertyChanged("Visibility");

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
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
