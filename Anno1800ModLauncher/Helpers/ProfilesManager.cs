using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anno1800ModLauncher.Helpers
{
    public class ProfilesManager : INotifyPropertyChanged
    {
        private ObservableCollection<Profile> profiles;
        public ObservableCollection<Profile> Profiles
        {
            get { return profiles; }
            set
            {
                profiles = value;
                OnPropertyChanged("Profiles");
            }
        }
        private string profilePath { get => Path.Combine(Properties.Settings.Default.GameRootPath, Properties.Settings.Default.ProfilesDirectory); }

        #region INotifyPropertyChanged Members

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

        public ProfilesManager()
        {
            Profiles = new ObservableCollection<Profile>(Directory.EnumerateFiles(profilePath).Select(
                e => new Profile(e)));
        }
    }

    public class Profile : INotifyPropertyChanged
    {
        private string name;
        private ObservableCollection<string> mods;

        public string Name { get => name; set => SetPropertyField("Name", ref name, value); }
        public ObservableCollection<string> Mods
        {
            get { return mods; }
            set
            {
                mods = value;
                OnPropertyChanged("Mods");
            }
        }

        public Profile(string path)
        {
            Name = Path.GetFileName(path);
            mods = new ObservableCollection<string>(File.ReadAllLines(path));
        }

        internal void Persist(string directory)
        {
            string path = Path.Combine(directory, Name);
            File.WriteAllLines(path, mods.ToArray());
        }

        protected void SetPropertyField<T>(string propertyName, ref T field, T newValue)
        {
            if (!EqualityComparer<T>.Default.Equals(field, newValue))
            {
                field = newValue;
                OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
            }
        }

        #region INotifyPropertyChanged Members

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


        public override string ToString()
        {
            return Name;
        }
    }
}
