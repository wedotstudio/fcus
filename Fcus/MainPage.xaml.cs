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
        private int oldp,p;

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
            
            //Remember Censor Position
            txt.Focus(FocusState.Pointer);
            p = txt.Document.Selection.StartPosition;

            if(oldp != p)
            {
                //Get document
                string docText;
                txt.Document.GetText(TextGetOptions.None, out docText);

                //Count Characters
                ccount.Text = (docText.Replace(" ", "").Length - 1) + " Character(s)";

                //Modify Styles
                Boldify(docText);

                //Recover Position
                txt.Focus(FocusState.Pointer);
                txt.Document.Selection.StartPosition = p;

                oldp = p;
            }    
        }
      
        private void Button_Click(object sender, RoutedEventArgs e)
        {
                    
        }
        private void Boldify(string docText)
        {
                txt.Document.GetText(TextGetOptions.None, out docText);
                string Boldpattern = @"\*{2}[^\s\*]+\*{2}";
                MatchCollection matches = Regex.Matches(docText, Boldpattern, RegexOptions.IgnoreCase);
                foreach (Match match in matches)
                {
                    string query = match.ToString();
                    var range = txt.Document.GetRange(0, docText.Length - 1);
                    int result = range.FindText(query, docText.Length - 1, Windows.UI.Text.FindOptions.None);

                    if (result == 0)
                    {
                        txt.Document.Selection.SetRange(0, 0);
                    }
                    else
                    {
                        txt.Document.Selection.SetRange(range.StartPosition, range.EndPosition);
                        range.CharacterFormat.Bold = FormatEffect.On;
                        //range.ScrollIntoView(Windows.UI.Text.PointOptions.None);
                    }
                }
            txt.Focus(FocusState.Pointer);
            txt.Document.Selection.StartPosition = p;
            txt.Document.GetDefaultCharacterFormat();
        }
    }
}
