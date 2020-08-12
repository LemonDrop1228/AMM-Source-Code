using Anno1800ModLauncher.Helpers;
using Anno1800ModLauncher.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Interaction logic for ModListView.xaml
    /// </summary>
    public partial class ModListView : UserControl, INotifyPropertyChanged
    {
        private ModDirectoryManager _modDirectoryManager;
        public ModDirectoryManager modDirectoryManager { get { return _modDirectoryManager; } set {
                _modDirectoryManager = value;
                OnPropertyChanged("modDirectoryManager");
            } }


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

        public ModListView()
        {
            InitializeComponent();
            DataContext = this;
            modDirectoryManager = new ModDirectoryManager();
            if (ModListBox.HasItems)
                ModListBox.SelectedIndex = 0;
            ModListBox.AllowDrop = true;
        }

        private void Activate_Mod(object sender, RoutedEventArgs e)
        {
            if (modDirectoryManager.modList != null && modDirectoryManager.modList.Count > 0 && ModListBox.SelectedItems.Count > 0)
            {
                var list = ModListBox.SelectedItems.Cast<ModModel>().ToList();
                list.ForEach(m => {
                    if (!m.IsActive)
                    {
                        if (modDirectoryManager.ActivateMod(m))
                        {
                            m.IsActive = true;
                            m.Icon = "CheckBold";
                            m.Color = "DarkGreen";                            
                        }
                    }
                });
                modDirectoryManager.LoadMods();
                FilterMods();
            }
            
        }

        private void Deactivate_Mod(object sender, RoutedEventArgs e)
        {
            if (modDirectoryManager.modList != null && modDirectoryManager.modList.Count > 0 && ModListBox.SelectedItems.Count > 0)
            {
                var list = ModListBox.SelectedItems.Cast<ModModel>().ToList();
                list.ForEach(m =>
                {
                    if (m.IsActive)
                    {
                        if (modDirectoryManager.DeactivateMod(m))
                        {
                            m.IsActive = false;
                            m.Icon = "NoEntry";
                            m.Color = "Red";
                        }
                    }
                });
                modDirectoryManager.LoadMods();
                FilterMods();
            }
        }

        private void ModListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ModListBox.Items.Count > 0 && ModListBox.SelectedIndex >= 0)
            {
                var i = ModListBox.SelectedItems.Count > 0 ? ModListBox.SelectedItems[ModListBox.SelectedItems.Count - 1] as ModModel : ModListBox.SelectedItem as ModModel;
                ReadMeTextBox.Text = modDirectoryManager.GetReadMeText(i);
                ModBannerImg.Source = modDirectoryManager.GetModBanner(i);
            }
        }

        internal void LoadList()
        {
            modDirectoryManager.LoadMods();
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ToggleButton button = (ToggleButton)sender;
            button.Background = Brushes.Salmon;
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            ToggleButton button = (ToggleButton)sender;
            button.Background = Brushes.DarkGreen;
        }

        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterMods();
        }

        private void FilterMods()
        {
            string filterText = FilterTextBox.Text;
            var filterStatusRaw = FilterCombo.SelectedValue;
            bool? filterStatus = filterStatusRaw == null ? null : ConvertStatus(filterStatusRaw as ComboBoxItem);

            modDirectoryManager.modList = modDirectoryManager.FilterMods(filterText, filterStatus);
        }

        private bool? ConvertStatus(ComboBoxItem filterStatusRaw)
        {
            switch (filterStatusRaw.Content)
            {
                case "Active":
                    return true;
                case "Inactive":
                    return false;
                default:
                    return false;
            }
        }

        private void FilterCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterMods();
        }

        private void ModListBox_SourceUpdated(object sender, DataTransferEventArgs e)
        {
        }

        private void ModListBox_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
        }

        private void ModListBox_DragEnter(object sender, DragEventArgs e)
        {
            //e.Effects = DragDropEffects.All;
        }

        private void ModListBox_Drop(object sender, DragEventArgs e)
        {
            //modDirectoryManager.DownloadInstallNewMod(e.Data.GetData(DataFormats.Text).ToString());
        }

        private void ModListBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //if (e.OriginalSource.GetType() == typeof(TextBlock))
            //{
            //    DependencyObject element = e.OriginalSource as DependencyObject;
            //    DependencyObject dependencyObject = ItemsControl.ContainerFromElement(sender as ListBox, element);
            //    if (dependencyObject != null)
            //    {
            //        var item = (dependencyObject as ListBoxItem).DataContext as ModModel;
            //        if (item != null)
            //        {
            //            ReadMeTextBox.Text = modDirectoryManager.GetReadMeText(item);
            //            ModBannerImg.Source = modDirectoryManager.GetModBanner(item);
            //        }
            //    } 
            //}
        }
    }
}
