using Anno1800ModLauncher.CustomDialogs;
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

        public static ProfilesManager Instance { get; private set; }

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
            Profiles = new ObservableCollection<Profile>(Directory.EnumerateFiles(profilesPath, "*.ammp").Select(
                e => new Profile(e)));
            Instance = this;
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
                p.UpdateMods(modDirectoryManager);
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
                Console.WriteLine("Successfully loaded profile: " + name);
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
                Console.WriteLine("Successfully deleted profile " + name);
            }
            else
            {
                Console.WriteLine(@"/!\ This profile doesn't exist!");
            }
        }

        internal void ImportProfile(string file)
        {
            string targetPath = Path.Combine(profilesPath, Path.GetFileName(file));
            if(File.Exists(targetPath))
            {
                ProfileImportDuplicateDialog dialog = new ProfileImportDuplicateDialog(() => ImportProfile(file, targetPath));
                dialog.Show();
            }
            else
            {
                ImportProfile(file, targetPath);
            }
        }

        private void ImportProfile(string file, string target)
        {
            File.Copy(file, target, true);
            if (profiles.Any(pr => pr.Name == Path.GetFileNameWithoutExtension(target)))
            {
                profiles.First(pr => pr.Name == Path.GetFileNameWithoutExtension(target)).Refresh(target);
            }
            else
            {
                profiles.Add(new Profile(target));
            }
            Console.WriteLine("Successfully imported the profile " + Path.GetFileNameWithoutExtension(file));
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
            Name = Path.GetFileNameWithoutExtension(path);
            mods = new ObservableCollection<string>(File.ReadAllLines(path));
        }
        public Profile(string name, ModDirectoryManager manager)
        {
            Name = name;
            mods = new ObservableCollection<string>(manager.modList.Where(m => m.IsActive).Select(m => m.Name));
        }

        internal void Persist(string directory)
        {
            string path = Path.Combine(directory, Name + ".ammp");
            File.WriteAllLines(path, mods.ToArray());
            Console.WriteLine("Successfully saved profile at " + path);
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

        internal void UpdateMods(ModDirectoryManager manager)
        {
            mods = new ObservableCollection<string>(manager.modList.Where(m => m.IsActive).Select(m => m.Name));
        }

        internal void Refresh(string path)
        {
            mods = new ObservableCollection<string>(File.ReadAllLines(path));
        }
    }
}
