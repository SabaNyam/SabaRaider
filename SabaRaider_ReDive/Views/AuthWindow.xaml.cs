using CoreTweet;
using MetroRadiance.UI.Controls;
using SabaRaider_ReDive.ViewModels;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;

namespace SabaRaider_ReDive.Views
{
    /// <summary>
    /// Interaction logic for AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : MetroWindow
    {
        AuthWindowViewModel vm;

        OAuth.OAuthSession session;
        Tokens tokens = null;

        string fileName = "userInfo.config";

        public AuthWindow()
        {
            InitializeComponent();

            vm = this.DataContext as AuthWindowViewModel;
        }

        // フォームを開いてtokensを返す静的メソッド
        static public Tokens ShowAuthForm()
        {
            AuthWindow authWindow = new AuthWindow();
            authWindow.ShowDialog();

            Tokens retTokens = authWindow.tokens;
            return retTokens;
        }

        private void IssuePIN(object sender, RoutedEventArgs e)
        {
            session = CoreTweet.OAuth.Authorize(Properties.Resources.APIKEY, Properties.Resources.APISEC);
            System.Diagnostics.Process.Start(session.AuthorizeUri.AbsoluteUri);
        }

        private void TwitterOAuth(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(vm.PINText.Value))
            {
                return;
            }

            tokens = OAuth.GetTokens(session, vm.PINText.Value);
            vm.PINText.Value = string.Empty;

            if (tokens == null)
            {
                MessageBox.Show("認証に失敗しました");
            }
            else
            {
                MessageBoxResult ret = MessageBox.Show("認証に成功しました。認証情報を保存しますか？", "", MessageBoxButton.YesNo);
                if (ret == MessageBoxResult.Yes)
                {
                    // トークン情報
                    AccessSettings accessSettings = new AccessSettings();
                    accessSettings.UserId = tokens.UserId;
                    accessSettings.ConsumerKey = tokens.ConsumerKey;
                    accessSettings.ConsumerSec = tokens.ConsumerSecret;
                    accessSettings.AccessKey = tokens.AccessToken;
                    accessSettings.AccessSec = tokens.AccessTokenSecret;

                    // バイナリファイル
                    BinaryFormatter bf = new BinaryFormatter();
                    System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Create);
                    bf.Serialize(fs, accessSettings);
                    fs.Close();
                }
            }

            this.Close();
        }
    }
}
