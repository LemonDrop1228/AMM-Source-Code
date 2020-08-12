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
using System.Windows.Shapes;
using Anno1800ModLauncher.Helpers.ModInstaller;
using MaterialDesignExtensions.Controls;

namespace Anno1800ModLauncher.CustomDialogs
{
    /// <summary>
    /// Interaction logic for ModInstallationDialog.xaml
    /// </summary>
    public partial class ModInstallationDialog : MaterialWindow, INotifyPropertyChanged
    {
        private ModInstallationManager modInstallationManager;
        private bool installing;
        private MessageBoxResult installationResult = MessageBoxResult.Cancel;

        public ModInstallationManager ModInstallationManager
        {
            get => modInstallationManager; set
            {
                modInstallationManager = value;
                modInstallationManager.InstallationComplete += ModInstallationManager_InstallationComplete;
                OnPropertyChanged("ModInstallationManager");
            }
        }

        private void ModInstallationManager_InstallationComplete(MessageBoxResult Results)
        {
            Installing = false;
            installationResult = Results;
            base.DialogResult = true;
        }

        public bool Installing { get => installing; set {
                installing = value;
                OnPropertyChanged("Installing");
            }
        }

        public MessageBoxResult ShowModDialog()
        {
            var b = this.ShowDialog();
            return installationResult;
        }

        private bool? ShowDialog()
        {
            return base.ShowDialog();
        }

        /// <summary>
        /// Raises the PropertyChanged notification in a thread safe manner
        /// </summary>
        /// <param name="propertyName"></param>
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



        public ModInstallationDialog()
        {
            InitializeComponent();
            DataContext = this;
        }

        public ModInstallationDialog(ModInstallationManager modInstallationManager)
        {
            InitializeComponent();
            DataContext = this;
            this.ModInstallationManager = modInstallationManager;
        }

        private void ActivateAll_Click(object sender, RoutedEventArgs e)
        {
            ModInstallationManager.ActivateAll();
        }

        private void InstallMods_Clicked(object sender, RoutedEventArgs e)
        {
            Installing = true;
            ModInstallationManager.Install();
        }
    }
}
