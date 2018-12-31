using SabaRaider_ReDive.Views;
using Prism.Ioc;
using System.Windows;
using MetroRadiance.UI;

namespace SabaRaider_ReDive
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //アプリを終了するため
            this.ShutdownMode = ShutdownMode.OnMainWindowClose;
            //起動時の色指定
            ThemeService.Current.Register(this, Theme.Dark, Accent.Blue);
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
    }
}
