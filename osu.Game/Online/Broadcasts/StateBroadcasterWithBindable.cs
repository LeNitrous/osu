// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;

namespace osu.Game.Online.Broadcasts
{
    public interface IStateBroadcasterWithCurrent
    {
        void Broadcast();
    }

    public abstract partial class StateBroadcasterWithBindable<TIn, TOut> : StateBroadcaster<TOut>, IStateBroadcasterWithCurrent
    {
        private IStateBroadcastServer? server;
        private readonly IBindable<TIn> bindable;

        public StateBroadcasterWithBindable(string type, IBindable<TIn> bindable)
            : base(type)
        {
            this.bindable = bindable.GetBoundCopy();
            this.bindable.ValueChanged += handleValueChanged;
        }

        protected abstract TOut GetData(TIn from);

        [BackgroundDependencyLoader]
        private void load(IStateBroadcastServer server)
        {
            this.server = server;
            this.server.Start(this);
        }

        private void handleValueChanged(ValueChangedEvent<TIn> value)
        {
            broadcast();
        }

        private void broadcast()
        {
            Schedule(() => Broadcast(GetData(bindable.Value)));
        }

        protected override void Dispose(bool isDisposing)
        {
            server?.Close(this);
            base.Dispose(isDisposing);
        }

        void IStateBroadcasterWithCurrent.Broadcast() => broadcast();
    }

    public partial class StateBroadcasterWithBindable<T> : StateBroadcasterWithBindable<T, T>
    {
        public StateBroadcasterWithBindable(string type, IBindable<T> bindable)
            : base(type, bindable)
        {
        }

        protected sealed override T GetData(T from) => from;
    }
}
