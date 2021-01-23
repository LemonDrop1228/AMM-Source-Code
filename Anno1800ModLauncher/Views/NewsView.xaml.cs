using CefSharp;
using CefSharp.Wpf;
using CefSharp.Wpf.Internals;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Anno1800ModLauncher.Helpers.Enums;
using Anno1800ModLauncher.Helpers;

namespace Anno1800ModLauncher.Views
{
    /// <summary>
    /// Interaction logic for NewsView.xaml
    /// </summary>
    public partial class NewsView : UserControl
    {

        public ChromiumWebBrowser webBrowser { get; private set; }

        public NewsView()
        {
            InitializeComponent();
        }

        private void NewsBrowser_Loaded(object sender, RoutedEventArgs e)
        {
            //make loading dependant on language
            ChromiumWebBrowser chromiumWebBrowser = (sender as ChromiumWebBrowser);
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                //if app isn't set the german and the address doesn't equal english URL 
                //or if app is set to german and the address doesn't equal german URL
                //then set language
                if (chromiumWebBrowser.Address == null ||
                    (!LanguageManager.GetLanguage().Equals(HelperEnums.Language.German) && !chromiumWebBrowser.Address.Contains(Properties.Settings.Default.NewsUrl)) ||
                     (LanguageManager.GetLanguage().Equals(HelperEnums.Language.German) && !chromiumWebBrowser.Address.Contains(Properties.Settings.Default.NewsUrlDE)))
                {
                    if (LanguageManager.GetLanguage().Equals(HelperEnums.Language.German))
                        chromiumWebBrowser.Load(Properties.Settings.Default.NewsUrlDE);
                    else
                        chromiumWebBrowser.Load(Properties.Settings.Default.NewsUrl);
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (NewsBrowser.WebBrowser == null)
            {
                NewsBrowser.WebBrowser = new ChromiumWebBrowser();
                NewsBrowser.WebBrowser.MenuHandler = new MenuHandler();
            }
            else
            {
                NewsBrowser.WebBrowser.MenuHandler = new MenuHandler();
            }
        }

        internal void LoadNews()
        {
            NewsBrowser.Load(Properties.Settings.Default.NewsUrl);
        }
    }



    public class MenuHandler : IContextMenuHandler
    {
        public void OnBeforeContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model)
        {
            model.Clear();
        }

        public bool OnContextMenuCommand(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, CefMenuCommand commandId, CefEventFlags eventFlags)
        {

            return false;
        }

        public void OnContextMenuDismissed(IWebBrowser browserControl, IBrowser browser, IFrame frame)
        {

        }

        public bool RunContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model, IRunContextMenuCallback callback)
        {
            return false;
        }
    }
}
