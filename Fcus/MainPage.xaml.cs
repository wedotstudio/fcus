using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Fcus
{
    public sealed partial class MainPage : Page
    {
        private List<ITextRange> _foundKeys = new List<ITextRange>();
        private int oldp, p;
        private ITextCharacterFormat _textFormat;
        private ITextParagraphFormat _paraFormat;
        public enum MarkdownFormat
        {
            None = 0,
            Bold = 1,
            Italic = 2,
            Image = 3,
            Link = 4,
            Headers = 5
        }

        public MainPage()
        {
            this.InitializeComponent();

            var applicationView = ApplicationView.GetForCurrentView();
            var titleBar = applicationView.TitleBar;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveForegroundColor = Colors.Gray;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonForegroundColor = Colors.Gray;

            Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
           _textFormat =  txt.Document.GetDefaultCharacterFormat();
            _paraFormat = txt.Document.GetDefaultParagraphFormat();
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            splitView.IsPaneOpen = !splitView.IsPaneOpen;
        }

        private void txt_TextChanged(object sender, RoutedEventArgs e)
        {
            UpdateRTB();       
        }

        private async void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            // Open a text file.
            Windows.Storage.Pickers.FileOpenPicker open =
                new Windows.Storage.Pickers.FileOpenPicker();
            open.SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            open.FileTypeFilter.Add(".md");
            open.FileTypeFilter.Add(".markdown");
            open.FileTypeFilter.Add(".text");
            open.FileTypeFilter.Add(".txt");
            open.FileTypeFilter.Add(".mmd");
            open.FileTypeFilter.Add(".mdown");

            Windows.Storage.StorageFile file = await open.PickSingleFileAsync();

            if (file != null)
            {
                try
                {
                    string text = await Windows.Storage.FileIO.ReadTextAsync(file);
                    txt.Document.SetText(TextSetOptions.None, text);
                    UpdateRTB();
                }
                catch (Exception)
                {
                    ContentDialog errorDialog = new ContentDialog()
                    {
                        Title = "File open error",
                        Content = "Sorry, I couldn't open the file.",
                        PrimaryButtonText = "Ok"
                    };

                    await errorDialog.ShowAsync();
                }
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Windows.Storage.Pickers.FileSavePicker savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;

            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("Markdown File", new List<string>() { ".md", ".markdown", ".mmd", ".mdown" });
            savePicker.FileTypeChoices.Add("Text Document", new List<string>() { ".text", ".txt" });

            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = "New Markdown Document";

            Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                string outText;
                txt.Document.GetText(TextGetOptions.None, out outText);
                await Windows.Storage.FileIO.WriteTextAsync(file, outText);
            }
        }

        private void FormatDocument(string docText,MarkdownFormat format )
        {
            string pattern = @"\w";
            txt.Document.GetText(TextGetOptions.None, out docText);
            switch (format)
            {
                case MarkdownFormat.Bold:
                    pattern = @"(\*\*|__)(.*?)\1";
                    break;
                case MarkdownFormat.Italic:
                    pattern = @"(\*|_)(.*?)\1";
                    break;
                case MarkdownFormat.Headers:
                    pattern = @"(#+)(.*?)";
                    break;
            }
             
            MatchCollection matches = Regex.Matches(docText, pattern, RegexOptions.None);
            foreach (Match match in matches)
            {
                string query = match.ToString();
                var range = txt.Document.GetRange(0, docText.Length - 1);
                int result = range.FindText(query, docText.Length, FindOptions.None);

                if (result == 0)
                {
                    txt.Document.Selection.SetRange(0, 0);
                }
                else
                {
                    txt.Document.Selection.SetRange(range.StartPosition, range.EndPosition);
                    switch (format)
                    {
                        case MarkdownFormat.Bold:
                            range.CharacterFormat.Bold = FormatEffect.On;
                            break;
                        case MarkdownFormat.Italic:
                            range.CharacterFormat.Italic = FormatEffect.On;
                            break;
                        case MarkdownFormat.Headers:
                            range.CharacterFormat.ForegroundColor = Colors.Purple;
                            range.CharacterFormat.Bold = FormatEffect.On;
                            break;
                    }
                    
                    //range.ScrollIntoView(Windows.UI.Text.PointOptions.None);
                }
            }
            txt.Focus(FocusState.Pointer);
            txt.Document.Selection.StartPosition = p;
        }
       
        private void UpdateRTB()
        {
            //Remember Censor Position
            txt.Focus(FocusState.Pointer);
            p = txt.Document.Selection.StartPosition;

            if (oldp != p)
            {
                //Get document
                string docText;
                txt.Document.GetText(TextGetOptions.None, out docText);

                //Reset Format
                var range = txt.Document.GetRange(0, docText.Length - 1);
                range.CharacterFormat.Bold = FormatEffect.Off;
                range.CharacterFormat.Italic = FormatEffect.Off;
                range.CharacterFormat.ForegroundColor = Colors.Black;

                //Count Characters
                ccount.Text = (docText.Replace(" ", "").Length - 1) + " Character(s)";

                //Modify Styles
                FormatDocument(docText, MarkdownFormat.Bold);
                FormatDocument(docText, MarkdownFormat.Italic);
                FormatDocument(docText, MarkdownFormat.Headers);

                //Recover Position
                txt.Focus(FocusState.Pointer);
                txt.Document.Selection.StartPosition = p;

                oldp = p;
            }
        }
       
    }
}
