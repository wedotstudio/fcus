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
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace Fcus
{
    public sealed partial class MainPage : Windows.UI.Xaml.Controls.Page
    {
        public string content = "";
        public StorageFile documentFile = null;
        public string documentTitle = "Welcome to Fcus!";

        public MainPage()
        {
            this.InitializeComponent();
            _initUI();

            content = "";
            documentTitle = "untitled";
            documentFile = null;

            editor.NavigationCompleted += Editor_NavigationCompleted;
        }

        private async void _initUI()
        {
            var applicationView = ApplicationView.GetForCurrentView();
            var titleBar = applicationView.TitleBar;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveForegroundColor = Colors.Gray;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonForegroundColor = Colors.Gray;

            Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;

            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var statusbar = StatusBar.GetForCurrentView();
                await statusbar.ShowAsync();
            }
        }

        private void Editor_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            NewWindowSetter();
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
            if (e.Value == "change") { OnCodeContentChanged(); NewWindowSetter(); }
            else await new MessageDialog(e.Value).ShowAsync();
        }

        private async void OnCodeContentChanged()
        {
            content = await editor.InvokeScriptAsync("getmd", null);
            
        }
        public async void NewFile()
        {
            content = "";
            documentTitle = "untitled";
            documentFile = null;
            mdtitle.Text = documentTitle;
            await editor.InvokeScriptAsync("setContent", new string[] { content });
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

            StorageFile file = await open.PickSingleFileAsync();
            var buffer = await FileIO.ReadBufferAsync(file);
            Encoding FileEncoding = SimpleHelpers.FileEncoding.DetectFileEncoding(buffer.AsStream(), Encoding.UTF8);
            var reader = new StreamReader(buffer.AsStream(), FileEncoding);

            content = reader.ReadToEnd().Replace("\r\n", "\n");
            documentFile = file;
            documentTitle = file.Name;
            mdtitle.Text = documentTitle;
            await editor.InvokeScriptAsync("setContent", new string[] { content });
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
                    documentTitle = file.Name;
                }
            }
            else
            {
                SaveDoc2File(documentFile);
            }
        }

        private async void SaveDoc2File(StorageFile file)
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
    }
}
