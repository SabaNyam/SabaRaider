using CoreTweet;
using CoreTweet.Streaming;
using MetroRadiance.UI;
using SabaRaider_ReDive.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SabaRaider_ReDive.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        MainWindowViewModel vm;

        // Twitterトークン
        public Tokens tokens = null;

        // 設定ファイル名
        string fileName = "userInfo.config";

        // ストリーミング接続
        private IDisposable StreamingConnection = null;

        public MainWindow()
        {
            InitializeComponent();

            vm = this.DataContext as MainWindowViewModel;

            this.Closing += (s, e) =>
            {
                vm.Dispose();
            };
        }

        private void MetroWindow_ContentRendered(object sender, System.EventArgs e)
        {
            try
            {
                // 設定ファイルが保存済み
                if (System.IO.File.Exists(fileName))
                {
                    // 設定ファイル読み込み
                    BinaryFormatter bf = new BinaryFormatter();
                    FileStream fs = new FileStream(fileName, FileMode.Open);
                    AccessSettings accessSettings = (AccessSettings)bf.Deserialize(fs);
                    fs.Close();

                    tokens = Tokens.Create(
                                accessSettings.ConsumerKey,
                                accessSettings.ConsumerSec,
                                accessSettings.AccessKey,
                                accessSettings.AccessSec);

                    if (tokens == null)
                    {
                        MessageBox.Show("認証に失敗しました");
                    }
                    else
                    {
                        var userinfo = tokens.Account.VerifyCredentials();
                        vm.CoopTwitterID.Value = userinfo.ScreenName;
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("予期せぬエラーが発生しました");
                return;
            }

        }

        private void OpenAuthWindow(object sender, RoutedEventArgs e)
        {
            Tokens rt = AuthWindow.ShowAuthForm();
            if (rt != null)
            {
                this.tokens = rt;
                var userinfo = tokens.Account.VerifyCredentials();
                vm.CoopTwitterID.Value = userinfo.ScreenName;
            }
        }

        private void RaiderON(object sender, RoutedEventArgs e)
        {
            if (tokens == null)
            {
                return;
            }

            vm.RaidTimer.Start();
            StartStreaming();
        }

        private void RaiderOFF(object sender, RoutedEventArgs e)
        {
            vm.RaidTimer.Stop();
            StopStreaming();
        }

        private void ClearTimer(object sender, RoutedEventArgs e)
        {
            vm.RaidTimer.Reset();
            vm.RaidTime.Value = "00:00:00";
            if(StreamingConnection != null)
            {
                vm.RaidTimer.Start();
            }
        }

        // ストリーミング開始
        public void StartStreaming()
        {
            if (StreamingConnection != null)
                return;

            try
            {
                if (vm.SelectedID.Value == 0) return;
                var battleInfo = vm.MultiBattleList.Where(x => x.BattleID == vm.SelectedID.Value).FirstOrDefault();
                if (battleInfo == null) return;

                string trackStr = String.Concat("参加者募集！ ", battleInfo.BattleNameJP.ToString(), ",", "I need backup! ", battleInfo.BattleNameEn.ToString());

                var param = new Dictionary<string, string>
                {
                    { "track", trackStr }
                };

                // TwitterからのStreamingMessageを発行するIObservable
                IObservable<StreamingMessage> iObservable = tokens.Streaming.FilterAsObservable(param);

                // raidSubjectへと救援ツイートを発行
                StreamingConnection = iObservable
                    .SubscribeOn(NewThreadScheduler.Default)
                    .Catch(
                        (Exception ex) => iObservable.DelaySubscription(TimeSpan.FromSeconds(5)).Retry(1))
                    .Repeat()
                    .Subscribe(x => {
                        var status = (x as StatusMessage).Status;

                        // 参戦IDを取得
                        string raidId = string.Empty;
                        if (0 <= status.Text.IndexOf("参戦ID"))
                        {
                            // comment xxxxxxxx :参戦ID 参加者募集！ Lv～
                            raidId = status.Text.Substring(status.Text.IndexOf("参戦ID") - 10, 8);
                        }
                        else if (0 <= status.Text.IndexOf("Battle ID"))
                        {
                            // comment xxxxxxxx :Battle ID I need Backup! Lvl～
                            raidId = status.Text.Substring(status.Text.IndexOf("Battle ID") - 10, 8);
                        }

                        Dispatcher.Invoke(() => TweetInfo.Content = String.Concat(raidId, "  ", "@", status.User.ScreenName, "  ", status.CreatedAt.AddHours(9).ToString("HH:mm:ss")));
                        Dispatcher.Invoke(() => Clipboard.SetText(raidId));
                    });

                this.RandomOira();
                this.RaidCombo.IsEnabled = false;
                // AccentColor変更
                ThemeService.Current.ChangeAccent(Accent.Orange);
            }
            catch (Exception)
            {
                this.StopStreaming();
            }
        }

        // ストリーミング停止
        public void StopStreaming()
        {
            if (StreamingConnection == null)
                return;

            try
            {
                StreamingConnection.Dispose();
            }
            catch
            {
                // Exception ignored
            }
            finally
            {
                // AccentColor変更
                ThemeService.Current.ChangeAccent(Accent.Blue);
                this.OiraImage.Source = new BitmapImage(new Uri(@"/Resources/oirastamp.png", UriKind.Relative));
                this.RaidCombo.IsEnabled = true;
                StreamingConnection = null;
            }
        }

        private void RandomOira()
        {
            Random r = new Random(CreateRandomSeed());
            int ran = r.Next(8);

            switch(ran)
            {
                case 0:
                    this.OiraImage.Source = new BitmapImage(new Uri(@"/Resources/oira1.jpg", UriKind.Relative));
                    break;
                case 1:
                    this.OiraImage.Source = new BitmapImage(new Uri(@"/Resources/oira2.jpg", UriKind.Relative));
                    break;
                case 2:
                    this.OiraImage.Source = new BitmapImage(new Uri(@"/Resources/oira3.jpg", UriKind.Relative));
                    break;
                case 3:
                    this.OiraImage.Source = new BitmapImage(new Uri(@"/Resources/oira4.jpg", UriKind.Relative));
                    break;
                case 4:
                    this.OiraImage.Source = new BitmapImage(new Uri(@"/Resources/oira5.jpg", UriKind.Relative));
                    break;
                case 5:
                    this.OiraImage.Source = new BitmapImage(new Uri("@/Resources/oira6.jpg", UriKind.Relative));
                    break;
                case 6:
                    this.OiraImage.Source = new BitmapImage(new Uri("@/Resources/oira7.jpg", UriKind.Relative));
                    break;
                case 7:
                    this.OiraImage.Source = new BitmapImage(new Uri("@/Resources/oira8.jpg", UriKind.Relative));
                    break;
            }
        }

        private int CreateRandomSeed()
        {
            var bs = new byte[4];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(bs);
            }
            return BitConverter.ToInt32(bs, 0);
        }
    }
}
