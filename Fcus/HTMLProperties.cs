using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Fcus
{
    class HTMLProperties
    {
        // "HtmlString" attached property for a WebView
        public static readonly DependencyProperty HtmlStringProperty =
           DependencyProperty.RegisterAttached("HtmlString", typeof(string), typeof(HTMLProperties), new PropertyMetadata("", OnHtmlStringChanged));

        // Getter and Setter
        public static string GetHtmlString(DependencyObject obj) { return (string)obj.GetValue(HtmlStringProperty); }
        public static void SetHtmlString(DependencyObject obj, string value) { obj.SetValue(HtmlStringProperty, value); }

        // Handler for property changes in the DataContext : set the WebView
        private static void OnHtmlStringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WebView wv = d as WebView;
            if (wv != null)
            {
                wv.NavigateToString((string)e.NewValue);
            }
        }

    }
}
