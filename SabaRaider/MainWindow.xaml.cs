using CoreTweet;
using CoreTweet.Streaming;
using MetroRadiance.UI;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SabaRaider
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow
    {
        public Tokens tokens = null;

        // 設定ファイル名
        string fileName = "userInfo.config";
        // csvファイル名
        string csvName = "RaidBattleList.csv";

        // DataTable
        ArrayList csvRecords = new ArrayList();

        // ストリーミング接続
        private IDisposable StreamingConnection = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        // ロード
        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // マルチバトルリスト読み込み
                using (Stream stream = new FileStream(csvName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (TextFieldParser parser = new TextFieldParser(stream, Encoding.Default))
                    {
                        parser.TextFieldType = FieldType.Delimited;
                        parser.Delimiters = new[] { "," };
                        parser.HasFieldsEnclosedInQuotes = true;
                        parser.TrimWhiteSpace = true;
                        while (!parser.EndOfData)
                        {
                            string[] fields = parser.ReadFields();
                            csvRecords.Add(fields);
                        }
                    }
                }
            }
            catch (IOException)
            {
                MessageBox.Show(csvName + "の読み込みに失敗しました");
                this.Close();
            }
            catch (Exception)
            {
                MessageBox.Show("予期せぬエラーが発生しました");
                this.Close();
            }

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
                        //MessageBox.Show("認証に成功しました");
                        var userinfo = tokens.Account.VerifyCredentials();

                        AccountLabel.Content = "連携中：" + userinfo.ScreenName;
                        RaidType1.IsChecked = true;
                    }
                }
            }
            catch(Exception)
            {
                MessageBox.Show("予期せぬエラーが発生しました");
                return;
            }
        }

        // 終了時
        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopStreaming();
        }

        // 認証画面の表示
        private void auth_Click(object sender, RoutedEventArgs e)
        {
            Tokens rt = AuthWindow.ShowAuthForm();
            if(rt != null)
            {
                this.tokens = rt;
                AccountLabel.Content = "連携中：" + tokens.Account.VerifyCredentials().ScreenName;
                RaidType1.IsChecked = true;
            }
        }

        // ラジオボタンチェック
        private void RaidType_Checked(object sender, RoutedEventArgs e)
        {
            string radioName = ((RadioButton)sender).Name;
            string raidType = radioName.Replace("RaidType", "");

            var res = csvRecords.Cast<string[]>().Where(x => raidType.Equals(x[0]));
            RaidCombo.ItemsSource = res.OrderBy(x => x[2], new LogicalStringComparer()).Select(x => x[1]);
        }

        // 取得開始
        private void RaidON_Click(object sender, RoutedEventArgs e)
        {
            if(tokens == null)
            {
                return;
            }

            StartStreaming();
        }

        // 取得終了
        private void RaidOFF_Click(object sender, RoutedEventArgs e)
        {
            StopStreaming();
        }

        // ストリーミング開始
        public void StartStreaming()
        {
            if (StreamingConnection != null)
                return;

            try
            {

                if (String.IsNullOrWhiteSpace(RaidCombo.SelectedValue.ToString()))
                    return;

                string trackStr = String.Concat("参加者募集！ ", RaidCombo.SelectedValue.ToString(), ",", "I need backup! ", RaidCombo.SelectedValue.ToString() );

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
                        if (0 <= status.Text.IndexOf("参戦ID"))
                        {
                            // comment xxxxxxxx :参戦ID 参加者募集！ Lv～
                            string raidId = status.Text.Substring(status.Text.IndexOf("参戦ID") - 10, 8);
                            Dispatcher.Invoke(() => TweetInfo.Content = String.Concat(raidId, "  ", "@", status.User.ScreenName, "  ", status.CreatedAt.AddHours(9).ToString("HH:mm:ss")));
                            Dispatcher.Invoke(() => Clipboard.SetText(raidId));
                        }
                        else if (0 <= status.Text.IndexOf("Battle ID"))
                        {
                            // comment xxxxxxxx :Battle ID I need Backup! Lvl～
                            string raidId = status.Text.Substring(status.Text.IndexOf("Battle ID") - 10, 8);
                            Dispatcher.Invoke(() => TweetInfo.Content = String.Concat(raidId, "  ", "@", status.User.ScreenName, "  ", status.CreatedAt.AddHours(9).ToString("HH:mm:ss")));
                            Dispatcher.Invoke(() => Clipboard.SetText(raidId));
                        }
                    });

                RaidPanel.IsEnabled = false;
                RaidCombo.IsEnabled = false;

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

                RaidPanel.IsEnabled = true;
                RaidCombo.IsEnabled = true;

                // AccentColor変更
                ThemeService.Current.ChangeAccent(Accent.Blue);
            }
            catch
            {
                // Exception ignored
            }
            finally
            {
                StreamingConnection = null;
            }
        }

        /// <summary>
        /// 文字列を自然順で比較するComparer
        /// </summary>
        public class LogicalStringComparer :
            System.Collections.IComparer,
            System.Collections.Generic.IComparer<string>
        {
            [System.Runtime.InteropServices.DllImport("shlwapi.dll",
                CharSet = System.Runtime.InteropServices.CharSet.Unicode,
                ExactSpelling = true)]
            private static extern int StrCmpLogicalW(string x, string y);

            public int Compare(string x, string y)
            {
                return StrCmpLogicalW(x, y);
            }

            public int Compare(object x, object y)
            {
                return this.Compare(x.ToString(), y.ToString());
            }
        }

    }
}
