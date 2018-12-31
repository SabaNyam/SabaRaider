using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace SabaRaider_ReDive.Base
{
    public abstract class ViewModelBase : IDisposable
    {
        protected CompositeDisposable Disposable { get; private set; }

        public ViewModelBase()
        {
            this.Disposable = new CompositeDisposable();
        }

        void IDisposable.Dispose()
        {
            this.Disposable.Dispose();
        }
    }
}
