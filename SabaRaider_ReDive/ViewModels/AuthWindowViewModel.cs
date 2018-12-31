using Prism.Commands;
using Prism.Mvvm;
using Reactive.Bindings;
using SabaRaider_ReDive.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SabaRaider_ReDive.ViewModels
{
    public class AuthWindowViewModel : ViewModelBase
    {
        public ReactiveProperty<string> PINText { get; set; } = new ReactiveProperty<string>();

        public AuthWindowViewModel()
        {

        }
    }
}
