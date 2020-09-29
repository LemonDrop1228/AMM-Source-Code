using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anno1800ModLauncher.Helpers
{
    public class ProfilesManager : INotifyPropertyChanged, IEnumerable<string>
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
        private string profilesPath
        {
            get
            {
                string ret = Path.Combine(Properties.Settings.Default.GameRootPath, Properties.Settings.Default.ProfilesDirectory);
                if(!Directory.Exists(ret))
                {
                    Directory.CreateDirectory(ret);
                }
                return ret;
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

        public ProfilesManager()
        {
            Profiles = new ObservableCollection<Profile>(Directory.EnumerateFiles(profilesPath).Select(
                e => new Profile(e)));
        }

        public IEnumerator<string> GetEnumerator()
        {
            return Profiles.Select(e => e.Name).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Profiles.Select(e => e.Name).GetEnumerator();
        }

        internal void Persist(string name, ModDirectoryManager modDirectoryManager)
        {
            Profile p;
            if (profiles.Any(pr => pr.Name == name))
            {
                p = profiles.First(profiles => profiles.Name == name);
            }
            else
            {
                p = new Profile(name, modDirectoryManager);
                profiles.Add(p);
            }
            p.Persist(profilesPath);
        }

        internal void Load(string name, ModDirectoryManager modDirectoryManager)
        {
            if (profiles.Any(pr => pr.Name == name))
            {
                Profile p = profiles.First(profiles => profiles.Name == name);
                p.Load(modDirectoryManager);
            }
            else
            {
                System.Console.WriteLine(@"/!\ This profile doesn't exist!");
            }
        }

        internal void Delete(string name)
        {
            if (profiles.Any(pr => pr.Name == name))
            {
                Profile p = profiles.First(profiles => profiles.Name == name);
                p.Delete(profilesPath);
                Profiles.Remove(p);
            }
            else
            {
                System.Console.WriteLine(@"/!\ This profile doesn't exist!");
            }
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
        public Profile(string name, ModDirectoryManager manager)
        {
            Name = name;
            mods = new ObservableCollection<string>(manager.modList.Where(m => m.IsActive).Select(m => m.Name));
        }

        internal void Persist(string directory)
        {
            string path = Path.Combine(directory, Name);
            File.WriteAllLines(path, mods.ToArray());
        }

        internal void Load(ModDirectoryManager modDirectoryManager)
        {
            foreach (ModModel m in modDirectoryManager.modList)
            {
                if(Mods.Contains(m.Name))
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
                }
                else
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
                }
            }
            modDirectoryManager.LoadMods();
        }

        internal void Delete(string directory)
        {
            string path = Path.Combine(directory, Name);
            File.Delete(path);
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
