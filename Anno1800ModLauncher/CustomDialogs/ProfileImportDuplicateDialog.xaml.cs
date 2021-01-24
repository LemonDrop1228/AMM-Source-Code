using Anno1800ModLauncher.Helpers;
using MaterialDesignExtensions.Controls;
using System;
using System.Collections.Generic;
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

namespace Anno1800ModLauncher.CustomDialogs
{
    /// <summary>
    /// Logique d'interaction pour ProfileImportDuplicateDialog.xaml
    /// </summary>
    public partial class ProfileImportDuplicateDialog : MaterialWindow
    {
        private Action action;

        public ProfileImportDuplicateDialog(Action action)
        {
            InitializeComponent();
            this.action = action;
        }

        private void Overwrite_Click(object sender, RoutedEventArgs e)
        {
            action.Invoke();
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
