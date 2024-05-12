using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Composition.SystemBackdrops;
using WinRT;
using Microsoft.UI;
using WinRT.Interop;
using Microsoft.UI.Windowing;
using Windows.ApplicationModel.Background;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PBA_App
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private AppWindow m_AppWindow;

        private AppWindow GetAppWindowForCurrentWindow()
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(wndId);
        }

        private WindowsSystemDispatcherQueueHelper? _wsdqHelper;

        private DesktopAcrylicController? _acrylicController;

        private MicaController? _micaController;

        private SystemBackdropConfiguration? _configurationSource;

        private void nvSample_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            FrameNavigationOptions navOptions = new FrameNavigationOptions();
            navOptions.TransitionInfoOverride = args.RecommendedNavigationTransitionInfo;
            if (sender.PaneDisplayMode == NavigationViewPaneDisplayMode.Top)
            {
                navOptions.IsNavigationStackEnabled = false;
            }
            Type pageType = typeof(Home);

            var selectedItem = (NavigationViewItem)args.SelectedItem;
            switch (selectedItem.Name)
            {
                case "NavItem_Home":
                    pageType = typeof(Home);
                    break;

                case "NavItem_Join":
                    pageType = typeof(Join);
                     break;
                case "NavItem_News":
                    pageType = typeof(News);
                    break;
                case "NavItem_Members":
                    pageType = typeof(Members);
                    break;
                case "NavItem_Groups":
                    pageType = typeof(Groups);
                    break;
                case "NavItem_Account":
                    pageType = typeof(Account);
                    break;
                default:
                    pageType = typeof(Settings);
                    break;
            }                
            _ = contentFrame.Navigate(pageType);
        }

        public MainWindow()
        {
            this.InitializeComponent();
            Title = $"PBA App";
            m_AppWindow = GetAppWindowForCurrentWindow();
            ExtendsContentIntoTitleBar = true;
            var result = TrySetMicaBackdrop();
        }

        private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            IntPtr windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(windowHandle);
            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);
            appWindow.SetIcon("PBA App.ico");
        }

        private bool TrySetMicaBackdrop()
        {
            if (MicaController.IsSupported() is true)
            {
                _wsdqHelper = new WindowsSystemDispatcherQueueHelper();
                _wsdqHelper.EnsureWindowsSystemDispatcherQueueController();

                // Hooking up the policy object
                _configurationSource = new SystemBackdropConfiguration();
                this.Activated += Window_Activated;
                this.Closed += Window_Closed;
                ((FrameworkElement)this.Content).ActualThemeChanged += Window_ThemeChanged;

                // Initial configuration state.
                _configurationSource.IsInputActive = true;
                SetConfigurationSourceTheme();

                _micaController = new MicaController();

                // Enable the system backdrop.
                // Note: Be sure to have "using WinRT;" to support the Window.As<...>() call.
                _micaController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                _micaController.SetSystemBackdropConfiguration(_configurationSource);
                return true; // succeeded
            }

            return false; // Mica is not supported on this system
        }

        private void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
            if (_configurationSource is not null)
            {
                _configurationSource.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
            }
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            // Make sure any Mica/Acrylic controller is disposed so it doesn't try to
            // use this closed window.
            if (_acrylicController is not null)
            {
                _acrylicController.Dispose();
                _acrylicController = null;
            }

            this.Activated -= Window_Activated;
            _configurationSource = null;
        }

        private void Window_ThemeChanged(FrameworkElement sender, object args)
        {
            if (_configurationSource is not null)
            {
                SetConfigurationSourceTheme();
            }
        }

        private void SetConfigurationSourceTheme()
        {
            if (_configurationSource is not null)
            {
                switch (((FrameworkElement)this.Content).ActualTheme)
                {
                    case ElementTheme.Dark:
                        _configurationSource.Theme = SystemBackdropTheme.Dark;
                        break;
                    case ElementTheme.Light:
                        _configurationSource.Theme = SystemBackdropTheme.Light;
                        break;
                    case ElementTheme.Default:
                        _configurationSource.Theme = SystemBackdropTheme.Default;
                        break;
                }
            }
        }
    }
}
