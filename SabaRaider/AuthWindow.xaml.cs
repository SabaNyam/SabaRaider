using CoreTweet;
using MetroRadiance.UI.Controls;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;

namespace SabaRaider
{
    /// <summary>
    /// AuthWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class AuthWindow : MetroWindow
    {
        OAuth.OAuthSession session;
        Tokens tokens = null;

        string fileName = "userInfo.config";

        public AuthWindow()
        {
            InitializeComponent();
        }

        // フォームを開いてtokensを返す静的メソッド
        static public Tokens ShowAuthForm()
        {
            AuthWindow authWindow = new AuthWindow();
            authWindow.ShowDialog();

            Tokens retTokens = authWindow.tokens;
            return retTokens;
        }

        // PIN発行
        private void pin_Click(object sender, RoutedEventArgs e)
        {
            session = CoreTweet.OAuth.Authorize(Properties.Resources.APIKEY, Properties.Resources.APISEC);
            System.Diagnostics.Process.Start(session.AuthorizeUri.AbsoluteUri);
        }

        // 認証
        private void auth_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(pinText.Text))
            {
                return;
            }

            string pin = pinText.Text;
            pinText.Text = String.Empty;
            tokens = OAuth.GetTokens(session, pin);

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
