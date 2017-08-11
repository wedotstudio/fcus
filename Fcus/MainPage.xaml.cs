using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Fcus_Restart
{
    public sealed partial class MainPage : Page
    {
        public string content = "";
        public IStorageFile documentFile = null;
        public string documentTitle = "untitled";
        public int filenewopend = 0;
        public bool isCtrlKeyPressed;

        public MainPage()
        {
            this.InitializeComponent();
            _initUI();

            content = "";
            documentTitle = "untitled";
            documentFile = null;

            editor.NavigationCompleted += Editor_NavigationCompleted;
        }
        IStorageFile actfile;

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

            initializeFrostedGlass(bgGrid);

            Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
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
        private async void Editor_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            NewWindowSetter();

            if (actfile != null)
            {
                await openfileasync(actfile);
                actfile = null;
            }
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
            if (e.Value == "changed") { if( filenewopend == 2  ){ OnCodeContentChanged(); } filenewopend = 2; NewWindowSetter(); }
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
        public async void NewFile()
        {
            content = "";
            documentTitle = "untitled";
            settitle();
            documentFile = null;
            mdtitle.Text = documentTitle;
            await editor.InvokeScriptAsync("setContent", new string[] { content });
        }

        private void settitle()
        {
            ApplicationView.GetForCurrentView().Title = documentTitle + " - Fcus";
        }

        private async void OpenFile()
        {
            // Open a text file.
            FileOpenPicker open =
                new FileOpenPicker();
            open.SuggestedStartLocation =
                PickerLocationId.DocumentsLibrary;
            open.FileTypeFilter.Add(".md");
            open.FileTypeFilter.Add(".markdown");
            open.FileTypeFilter.Add(".text");
            open.FileTypeFilter.Add(".txt");
            open.FileTypeFilter.Add(".mmd");
            open.FileTypeFilter.Add(".mdown");

            await openfileasync(await open.PickSingleFileAsync());
        }


        private async System.Threading.Tasks.Task openfileasync(IStorageFile file)
        {
            var buffer = await FileIO.ReadBufferAsync(file);
            Encoding FileEncoding = SimpleHelpers.FileEncoding.DetectFileEncoding(buffer.AsStream(), Encoding.UTF8);
            var reader = new StreamReader(buffer.AsStream(), FileEncoding);

            content = reader.ReadToEnd().Replace("\r\n", "\n");
            documentFile = file;
            documentTitle = file.Name;
            settitle();
            mdtitle.Text = documentTitle;
            await editor.InvokeScriptAsync("setContent", new string[] { content });
        }

        private void FullScreen()
        {
            var view = ApplicationView.GetForCurrentView();
            if (view.IsFullScreenMode)
            {
                view.ExitFullScreenMode();
                fs_icon.Glyph = "";
            }
            else
            {
                view.TryEnterFullScreenMode();
                fs_icon.Glyph = "";
            }
        }

        private async void SaveFile()
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
                    settitle();
                    documentTitle = file.Name + ".md";
                }
            }
            else
            {
                SaveDoc2File(documentFile);
            }
        }

        private async void SaveDoc2File(IStorageFile file)
        {
            content = await editor.InvokeScriptAsync("getmd", null);
            var bytes = Encoding.UTF8.GetBytes(content);
            await FileIO.WriteBytesAsync(file, bytes);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            NewFile();
        }

        private void FullScreen_Click(object sender, RoutedEventArgs e)
        {
            FullScreen();
        }

        private void Grid_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Control) isCtrlKeyPressed = false;
        }

        private void Grid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Control) isCtrlKeyPressed = true;
            else if (isCtrlKeyPressed)
            {
                switch (e.Key)
                {
                    case VirtualKey.N: NewFile(); break;
                    case VirtualKey.O: OpenFile(); break;
                    case VirtualKey.S: SaveFile(); break;
                    case VirtualKey.F: FullScreen(); break;
                }
            }
        }
    }
}
