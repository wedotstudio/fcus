using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.System.Profile;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Navigation;

namespace Fcus_Restart
{
    public sealed partial class MainPage : Page
    {
        public string content;
        public IStorageFile documentFile;
        public IStorageFile actfile;
        public string documentTitle;
        public ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public bool ismobile = false;

        public MainPage()
        {
            this.InitializeComponent();
           
            _initUI();
            _initVar();
            
            Loaded += MainPage_Loaded;
            editor.NavigationCompleted += Editor_NavigationCompleted;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/about.md"));
            aboutText.Text = await FileIO.ReadTextAsync(file);
        }
        private async void Editor_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            NewWindowSetter();
            if(ismobile) await editor.InvokeScriptAsync("mobtweak", null);
            if (actfile != null)
            {
                await OpenFileTask(actfile);
                actfile = null;
            }
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var file = e.Parameter as IStorageFile;
            if (file != null)
            {
                //await openfileasync(file);
                actfile = file;
            }
        }

        private void _initUI()
        {
            var applicationView = ApplicationView.GetForCurrentView();
            var titleBar = applicationView.TitleBar;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveForegroundColor = Colors.DarkGray;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonForegroundColor = Colors.Black;
            if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile") {
                ismobile = true; 
                var statusBar = StatusBar.GetForCurrentView();
                statusBar.BackgroundOpacity = 1;
                statusBar.BackgroundColor = Color.FromArgb(255, 243,243,243);
                statusBar.ForegroundColor = Colors.Black;
                fs.Visibility = Visibility.Collapsed;
                cf.Opacity = 1;
                ip.Opacity = 1;
                }
            else {
                initializeFrostedGlass(bgGrid);
            }

            Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;

           
        }
        private void _initVar()
        {
            content = "";
            localSettings.Values["filestate"] = 0;
            documentTitle = "untitled";
            documentFile = null;
        }
        private void initializeFrostedGlass(UIElement glassHost)
        {
            Visual hostVisual = ElementCompositionPreview.GetElementVisual(glassHost);
            Compositor compositor = hostVisual.Compositor;
            var backdropBrush = compositor.CreateHostBackdropBrush();
            var glassVisual = compositor.CreateSpriteVisual();
            glassVisual.Brush = backdropBrush;
            ElementCompositionPreview.SetElementChildVisual(glassHost, glassVisual);
            var bindSizeAnimation = compositor.CreateExpressionAnimation("hostVisual.Size");
            bindSizeAnimation.SetReferenceParameter("hostVisual", hostVisual);
            glassVisual.StartAnimation("Size", bindSizeAnimation);
        }

        private async void NewWindowSetter()
        {
            await editor.InvokeScriptAsync("eval", new[]
           {
                @"(function()
                {
                    var hyperlinks = document.getElementsByTagName('a');
                    for(var i = 0; i < hyperlinks.length; i++)
                    {
                        hyperlinks[i].setAttribute('target', '_blank');
                    }
                })()"
            });
        }
        private async void ScriptNotify(object sender, NotifyEventArgs e)
        {
            if (e.Value == "changed") { if (Convert.ToInt32(localSettings.Values["filestate"]) == 2) { OnCodeContentChanged(); } localSettings.Values["filestate"] = 2; NewWindowSetter(); }
            else if (e.Value == "newfile") { NewFile(); }
            else if (e.Value == "openfile") { OpenFile(); }
            else if (e.Value == "savefile") { SaveFile(); }
            else if (e.Value == "full") { FullScreen(); }
            else if (e.Value == "toggle") { TogglePreview(); }
            else await new MessageDialog(e.Value).ShowAsync();
        }

        private async void OnCodeContentChanged()
        {

            if (!mdtitle.Text.EndsWith("*"))
            {
                mdtitle.Text += "*";
            }
            content = await editor.InvokeScriptAsync("getmd", null);
        }

        private void SetTitle()
        {
            ApplicationView.GetForCurrentView().Title = documentTitle + " - Fcus";
        }
        private async Task OpenFileTask(IStorageFile file)
        {
            if (file != null)
            {
                localSettings.Values["filestate"] = 1;
                var buffer = await FileIO.ReadBufferAsync(file);
                Encoding FileEncoding = SimpleHelpers.FileEncoding.DetectFileEncoding(buffer.AsStream(), Encoding.UTF8);
                var reader = new StreamReader(buffer.AsStream(), FileEncoding);

                content = reader.ReadToEnd().Replace("\r\n", "\n");
                documentFile = file;
                documentTitle = file.Name;
                SetTitle();
                mdtitle.Text = documentTitle;
                await editor.InvokeScriptAsync("setContent", new string[] { content });
            }
        }

        public async void NewFile()
        {
            if (Convert.ToInt32(localSettings.Values["filestate"]) == 2 && content != "")
            {
                var dlg = new MessageDialog("Current file is not saved. Do you want to save?", documentTitle);
                dlg.Commands.Add(new UICommand("Save", cmd => { SaveFile(); NewDoc(); }));
                dlg.Commands.Add(new UICommand("Discard", cmd => { NewDoc(); }));
                if(!ismobile) dlg.Commands.Add(new UICommand("Cancel"));
                await dlg.ShowAsync();
            }
            else
            {
                NewDoc();
            }
        }
        private async void NewDoc()
        {
            _initVar();
            SetTitle();
            mdtitle.Text = documentTitle;
            await editor.InvokeScriptAsync("setContent", new string[] { content });
        }
        
        private async void OpenFile()
        {
            if (Convert.ToInt32(localSettings.Values["filestate"]) == 2 && content != "")
            {
                var dlg = new MessageDialog("Current file is not saved. Do you want to save?", documentTitle);
                dlg.Commands.Add(new UICommand("Save", cmd => { SaveFile(); OpenDoc(); }));
                dlg.Commands.Add(new UICommand("Discard", cmd => { OpenDoc(); }));
                if (!ismobile) dlg.Commands.Add(new UICommand("Cancel"));
                await dlg.ShowAsync();
            }
            else
            {
                OpenDoc();
            }
        }
        private async void OpenDoc()
        {
            // Open a text file.

            FileOpenPicker open = new FileOpenPicker();
            open.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            open.FileTypeFilter.Add(".md");
            open.FileTypeFilter.Add(".markdown");
            open.FileTypeFilter.Add(".text");
            open.FileTypeFilter.Add(".txt");
            open.FileTypeFilter.Add(".mmd");
            open.FileTypeFilter.Add(".mdown");

            await OpenFileTask(await open.PickSingleFileAsync());
        }
   
        public async void SaveFile()
        {
            if (documentFile == null)
            {
                var picker = new FileSavePicker();
                picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                picker.FileTypeChoices.Add("Markdown File", new List<string>() { ".md", ".markdown", ".mmd", ".mdown" });
                picker.FileTypeChoices.Add("Text Document", new List<string>() { ".text", ".txt" }); picker.SuggestedFileName = documentTitle;
                StorageFile file = await picker.PickSaveFileAsync();
                if (file != null)
                {
                    SaveDoc2File(file);
                    documentFile = file;
                    SetTitle();
                    documentTitle = file.Name + ".md";
                    localSettings.Values["filestate"] = 1;
                }
            }
            else
            {
                SaveDoc2File(documentFile);
                mdtitle.Text = documentTitle;
                localSettings.Values["filestate"] = 1;
            }
        }
        private async void SaveDoc2File(IStorageFile file)
        {
            content = await editor.InvokeScriptAsync("getmd", null);
            var bytes = Encoding.UTF8.GetBytes(content);
            await FileIO.WriteBytesAsync(file, bytes);
        }

        private void FullScreen()
        {
            var view = ApplicationView.GetForCurrentView();
            if (view.IsFullScreenMode)
            {
                view.ExitFullScreenMode();
                fs.Content = "";
                InfoPanel.Text = "Fullscreen Mode Exited.";
            }
            else
            {
                view.TryEnterFullScreenMode();
                fs.Content = "";
                InfoPanel.Text = "Entered Fullscreen Mode. Press the button again or F11/Ctrl-F to exit.";
            }


        }
        private async void TogglePreview()
        {
            await editor.InvokeScriptAsync("toggle", null);
        }

        private void Save_Click(object sender, RoutedEventArgs e) {SaveFile(); }
        private void Open_Click(object sender, RoutedEventArgs e) {OpenFile(); }
        private void New_Click(object sender, RoutedEventArgs e) {NewFile(); }
        private void FullScreen_Click(object sender, RoutedEventArgs e) {FullScreen(); }
        private void Close_Click(object sender, RoutedEventArgs e) {aboutPanel.Visibility = Visibility.Collapsed; }
        private void About_Click(object sender, RoutedEventArgs e) {aboutPanel.Visibility = Visibility.Visible; }
        private void Preview_Click(object sender, RoutedEventArgs e) {TogglePreview(); }
        private async void About_Link_Click(object sender, Microsoft.Toolkit.Uwp.UI.Controls.LinkClickedEventArgs e) { await Launcher.LaunchUriAsync(new Uri(e.Link)); }
        private async void Recover_Click(object sender, RoutedEventArgs e) { if (localSettings.Values["backup_data"] != null) {
                content = Convert.ToString(localSettings.Values["backup_data"]);
                localSettings.Values["filestate"] = 2;
                documentTitle = Convert.ToString(localSettings.Values["backup_title"]);
                await editor.InvokeScriptAsync("setContent", new string[] { content });
            } }
    }
}
