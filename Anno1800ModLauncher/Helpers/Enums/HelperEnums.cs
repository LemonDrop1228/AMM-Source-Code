using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows; 

namespace Anno1800ModLauncher.Helpers.Enums
{
    public static class HelperEnums
    {
        public enum DetectionState
        {
            None,
            GameWithoutModContent,
            GameWithModsButNoLoader,
            GameWithLoaderButNoModFolder,
            Golden,
            GoldenButOld,
            GameWithOldLoaderButNoModFolder,
            InstallingModLoader

        }

        public enum ManagerStatus
        {
            good, bad
        }

        public enum Language
        {
            English,
            German,
            Chinese,
            French,
            Italian,
            Japanese,
            Korean,
            Polish,
            Russian,
            Spanish,
            Taiwanese
        }
    }
}
