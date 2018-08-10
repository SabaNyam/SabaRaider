using MetroRadiance.UI;
using System.Windows;

namespace SabaRaider
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //アプリを終了するため
            this.ShutdownMode = ShutdownMode.OnMainWindowClose;
            //起動時の色指定
            ThemeService.Current.Register(this, Theme.Dark, Accent.Blue);
        }
    }
}
