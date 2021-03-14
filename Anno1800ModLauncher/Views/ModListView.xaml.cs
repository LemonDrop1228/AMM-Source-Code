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
using Newtonsoft;
using Newtonsoft.Json;

namespace Anno1800ModLauncher.Views
{
    /// <summary>
    /// Interaction logic for ModListView.xaml
    /// </summary>
    public partial class ModListView : UserControl, INotifyPropertyChanged
    {
        private ModDirectoryManager _modDirectoryManager;
        public ModDirectoryManager modDirectoryManager
        {
            get { return _modDirectoryManager; }
            set
            {
                _modDirectoryManager = value;
                OnPropertyChanged("modDirectoryManager");
            }
        }

        private ImageSource OriginalBannerSource;
        private ProfilesManager _profileManager;

        //bindable Text for Mod Descriptions
        public string _modDescriptionText;
        public string modDescriptionText
        {
            get { return _modDescriptionText;  }
            set 
            {
                _modDescriptionText = value;
                OnPropertyChanged("modDescriptionText");
            }
        }

        public ProfilesManager profilesManager
        {
            get { return _profileManager; }
            set
            {
                _profileManager = value;
                OnPropertyChanged("profileManager");
            }
        }

        private String _activeMods { get; set; }

        public String activeMods
        {
            get { return _activeMods; }
            set
            {
                _activeMods = value;
                OnPropertyChanged("activeMods");
            }
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

        public ModListView()
        {
            InitializeComponent();
            DataContext = this;
            modDirectoryManager = new ModDirectoryManager();
            if (ModListBox.HasItems)
                ModListBox.SelectedIndex = 0;
            ModListBox.AllowDrop = true;
            profilesManager = new ProfilesManager();
            SetProfilesOptions();
            OriginalBannerSource = ModBannerImg.Source;
            LanguageManager.Instance.LanguageChanged += LanguageChanged;
            activeMods = "500";
        }

        private void SetProfilesOptions()
        {
            while (ProfileCombo.Items.Count > 1)
            {
                ProfileCombo.Items.RemoveAt(1);
            }

            foreach (string profile in profilesManager)
            {
                ProfileCombo.Items.Add(new ComboBoxItem() { Content = profile });
            }
        }

        private void Activate_Mod(object sender, RoutedEventArgs e)
        {
            if (modDirectoryManager.modList != null && modDirectoryManager.modList.Count > 0 && ModListBox.SelectedItems.Count > 0)
            {
                var list = ModListBox.SelectedItems.Cast<ModModel>().ToList();
                list.ForEach(m =>
                {
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

        private void LanguageChanged(object source, EventArgs args) {
            ModListBox_SelectionChanged(source, null);
        }

        private void ModListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ModListBox.Items.Count > 0 && ModListBox.SelectedIndex >= 0)
            {
                var i = ModListBox.SelectedItems.Count > 0 ? ModListBox.SelectedItems[ModListBox.SelectedItems.Count - 1] as ModModel : ModListBox.SelectedItem as ModModel;

                //use bound variable modDescriptionText instead of text setting here for binding to ReadMeTextBox in ModListView.xaml
                modDescriptionText = modDirectoryManager.GetReadMeText(i);
                //ReadMeTextBox.Text = modDirectoryManager.GetReadMeText(i);

                ModBannerImg.Source = modDirectoryManager.GetModBanner(i) ?? OriginalBannerSource;
            }
        }

        private void Reload_Mods(object sender, RoutedEventArgs e)
        {
            modDirectoryManager.LoadMods(); 
        }

        private void Select_All(object sender, RoutedEventArgs e)
        {
            ModListBox.SelectAll(); 
        }

        private void Save_Profile(object sender, RoutedEventArgs e)
        {
            string name;
            if(ProfileCombo.SelectedItem == newProfileItem)
            {
                name = ProfileTextBox.Text;
                ProfileTextBox.Text = "";
            }
            else
            {
                name = (string)((ComboBoxItem)ProfileCombo.SelectedItem).Content;
            }
            profilesManager.Persist(name, modDirectoryManager);
            SetProfilesOptions();
            foreach (ComboBoxItem item in ProfileCombo.Items)
            {
                if(name == (string)item.Content)
                {
                    ProfileCombo.SelectedItem = item;
                    break;
                }
            }

        }

        private void Load_Profile(object sender, RoutedEventArgs e)
        {
            profilesManager.Load(ProfileCombo.Text, modDirectoryManager);
            FilterMods();
        }

        private void Delete_Profile(object sender, RoutedEventArgs e)
        {
            profilesManager.Delete(ProfileCombo.Text);
            ProfileCombo.SelectedIndex = -1;
            SetProfilesOptions();
        }

        private void NewProfile_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ProfileTextBox.Text != "")
            {
                ProfileCombo.SelectedItem = newProfileItem;
                SaveProfile.IsEnabled = true;
                LoadProfile.IsEnabled = false;
                DeleteProfile.IsEnabled = false;
            }
            else
            {
                ProfileCombo.SelectedIndex = -1;
            }
        }

        private void ProfileCOmbo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProfileCombo.SelectedIndex == -1)// no selection
            {
                SaveProfile.IsEnabled = false;
                LoadProfile.IsEnabled = false;
                DeleteProfile.IsEnabled = false;
            }
            else if (ProfileCombo.SelectedItem == newProfileItem && ProfileTextBox.Text != "")// named new profile
            {
                SaveProfile.IsEnabled = true;
                LoadProfile.IsEnabled = false;
                DeleteProfile.IsEnabled = false;
            }
            else// existing profile
            {
                SaveProfile.IsEnabled = true;
                LoadProfile.IsEnabled = true;
                DeleteProfile.IsEnabled = true;
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
            /*switch (filterStatusRaw.ToString())
            {
                case FilterComboBoxActive.ToString():
                    return true;
                case FilterComboBoxInactive.ToString():
                    return false;
                default:
                    return false;
            }*/
            if (filterStatusRaw.Equals(FilterComboBoxActive)) {
                return true;
            }
            else if (filterStatusRaw.Equals(FilterComboBoxInactive))
            {
                return false;
            }
            return false; 
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

        public string GetModExportFile()
        {
            var modData = JsonConvert.SerializeObject(modDirectoryManager.modList);
            return modData.Base64Encode();
        }

        internal void UpdateByImport(ObservableCollection<ModModel> modListObj)
        {
            modDirectoryManager.modList.ForEach( mod =>
                {
                    var impMod = modListObj.FirstOrDefault(m => m.Name == mod.Name);
                    if (impMod != null)
                    {
                        if (impMod.IsActive && !mod.IsActive){
                            modDirectoryManager.ActivateMod(mod);
                            modDirectoryManager.LoadMods();
                            FilterMods();
                        }
                        else if (!impMod.IsActive && mod.IsActive) { 
                            modDirectoryManager.DeactivateMod(mod);
                            modDirectoryManager.LoadMods();
                            FilterMods();
                        }
                    }
                }
            );
        }

        private void ReadMeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
