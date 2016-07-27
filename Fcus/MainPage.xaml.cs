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
        private Color _highLihgtColor = Color.FromArgb(255, 150, 190, 255);

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
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            splitView.IsPaneOpen = !splitView.IsPaneOpen;
        }

        private void txt_TextChanged(object sender, RoutedEventArgs e)
        {
            string docText;
            txt.Document.GetText(TextGetOptions.None, out docText);
            ccount.Text = (docText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length-1) + " Word(s)";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string docText, _oldQuery;
            string Boldpattern = @"\*{2}[^\s\*][^\*]+[^\s\*]\*{2}";
            txt.Document.GetText(TextGetOptions.None, out docText);
            
            MatchCollection matches = Regex.Matches(docText, Boldpattern, RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                var start = txt.Document.Selection.EndPosition;
                var end = docText.Length;
                var range = txt.Document.GetRange(start, end);
                int result = range.FindText(match.ToString(), end - start, FindOptions.None);
                var xrange = txt.Document.GetRange(start,end);
                _foundKeys.Add(xrange);
            }            
        }
    }
}
