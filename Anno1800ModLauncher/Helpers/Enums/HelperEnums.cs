using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
