using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SabaRaider_ReDive.Base;
using SabaRaider_ReDive.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;

namespace SabaRaider_ReDive.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ReactiveProperty<string> CoopTwitterID { get; private set; } = new ReactiveProperty<string>("-");

        public List<MultiBattle> MultiBattleList { get; set; }
        public ObservableCollection<MultiBattle> MultiBattles { get; set; }
        public ReactiveProperty<int> SelectedID { get; set; } = new ReactiveProperty<int>(0);
        public ReactiveProperty<string> SearchText { get; set; } = new ReactiveProperty<string>(string.Empty);

        public ReactiveTimer RaidTimer { get; private set; }
        public ReactiveProperty<string> RaidTime { get; private set; } = new ReactiveProperty<string>("00:00:00");

        public MainWindowViewModel()
        {
            // リスト取得
            MultiBattleList = MultiBattle.GetMultiBattles();
            MultiBattles = new ObservableCollection<MultiBattle>(MultiBattleList);

            // 入力値からリスト検索
            SearchText.Subscribe(x => {
                var search = MultiBattleList.Where(s => s.DisplayBattleName.Contains(x)).FirstOrDefault();
                SelectedID.Value = string.IsNullOrWhiteSpace(x) ? 0 : search != null ? search.BattleID : 0;
            }).AddTo(Disposable);
            
            // タイマー制御
            RaidTimer = new ReactiveTimer(TimeSpan.FromSeconds(1));
            RaidTimer.Subscribe(x => {
                var span = new TimeSpan(0, 0, (int)x);
                RaidTime.Value = span.ToString(@"hh\:mm\:ss");
            }).AddTo(Disposable);
        }

        public void Dispose()
        {
            Disposable.Dispose();
        }
    }
}
