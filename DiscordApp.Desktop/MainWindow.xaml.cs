using Microsoft.Web.WebView2.Core;
using System;
using System.Windows;

namespace DiscordApp.Desktop
{
    public partial class MainWindow : Window
    {
        // Sitenin adresi burada. Kendi bilgisayarında test ederken bu satırı kullan.
        // Arkadaşlarınla paylaşırken Cloudflare Tunnel'dan aldığın adresle değiştirirsin.
        private const string AppUrl = "https://localhost:7200";

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await Browser.EnsureCoreWebView2Async(null);

                // Kendi imzalı (self-signed) sertifikan olduğu için tarayıcı SSL uyarısı verebilir,
                // sadece localhost için bu uyarıyı görmezden gelmesini sağlıyoruz.
                Browser.CoreWebView2.ServerCertificateErrorDetected += (s, args) =>
                {
                    if (AppUrl.Contains("localhost"))
                    {
                        args.Action = CoreWebView2ServerCertificateErrorAction.AlwaysAllow;
                    }
                };

                Browser.CoreWebView2.NavigationCompleted += (s, args) =>
                {
                    LoadingText.Visibility = Visibility.Hidden;
                    Browser.Visibility = Visibility.Visible;
                };

                Browser.Source = new Uri(AppUrl);
            }
            catch (Exception ex)
            {
                LoadingText.Text = $"Bağlantı kurulamadı: {ex.Message}\n\nWebApi ve WebUI projelerinin çalıştığından emin ol.";
            }
        }
    }
}