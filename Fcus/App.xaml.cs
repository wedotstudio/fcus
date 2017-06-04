using System;
using Microsoft.Azure.Mobile;
using Microsoft.Azure.Mobile.Analytics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Fcus
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            MobileCenter.Start("3ceb4858-3b7c-45f4-8883-0d99578c9ad8", typeof(Analytics));
            OnActivated(e);
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
        protected async override void OnActivated(IActivatedEventArgs args)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif


            var prelaunchActivated = false;


            var launcharg = args as LaunchActivatedEventArgs;
            if (launcharg != null)
            {
                prelaunchActivated = launcharg.PrelaunchActivated;
            }

            var vspro = args as IViewSwitcherProvider;
            if (vspro.ViewSwitcher == null)
            {
                Windows.UI.ViewManagement.ApplicationViewSwitcher.DisableSystemViewActivationPolicy();

                // Initialize rootFrame normally.
                CreateRootFrame(args, prelaunchActivated);

            }
            else
            {
                // Create new view, use launcharg.ViewSwitcher.ShowAsStandaloneAsync(int) to display.
                var view = Windows.ApplicationModel.Core.CoreApplication.CreateNewView();
                await view.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    CreateRootFrame(args, prelaunchActivated);
                    var id = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().Id;
                    await vspro.ViewSwitcher.ShowAsStandaloneAsync(id);
                });

                return;
            }

        }
        private void CreateRootFrame(IActivatedEventArgs args, bool prelaunchActivated)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }


            var currentView = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
            currentView.Consolidated += CurrentView_Consolidated;
            if (!prelaunchActivated)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                var filearg = args as FileActivatedEventArgs;

                if (filearg == null && rootFrame.Content == null)
                    rootFrame.Navigate(typeof(MainPage));
                else
                    rootFrame.Navigate(typeof(MainPage), filearg.Files[0] as Windows.Storage.IStorageFile);
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        private void CurrentView_Consolidated(Windows.UI.ViewManagement.ApplicationView sender, Windows.UI.ViewManagement.ApplicationViewConsolidatedEventArgs args)
        {
            Window.Current.Content = null;
        }

        protected override void OnFileActivated(FileActivatedEventArgs args)
        {
            OnActivated(args);
        }
    }
}
