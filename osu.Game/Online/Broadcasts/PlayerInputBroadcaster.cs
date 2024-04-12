// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Online.Broadcasts
{
    public sealed partial class PlayerInputBroadcaster : StateBroadcaster<string>
    {
        private readonly InputCountController input;
        private readonly Dictionary<string, InputTrigger.OnActivateCallback> callbacks = new Dictionary<string, InputTrigger.OnActivateCallback>();

        public PlayerInputBroadcaster(InputCountController input)
            : base(BroadcastID.PLAY_INPUT)
        {
            this.input = input;
            this.input.Triggers.CollectionChanged += handleCollectionChanged;
        }

        private void handleCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.OldItems is not null)
            {
                foreach (var trigger in args.OldItems.Cast<InputTrigger>())
                {
                    if (!callbacks.TryGetValue(trigger.Name, out var callback))
                    {
                        continue;
                    }

                    trigger.OnActivate -= callback;
                }
            }

            if (args.NewItems is not null)
            {
                foreach (var trigger in args.NewItems.Cast<InputTrigger>())
                {
                    var callback = new InputTrigger.OnActivateCallback(isForward =>
                    {
                        if (isForward)
                        {
                            Broadcast(trigger.Name);
                        }
                    });

                    callbacks.Add(trigger.Name, callback);
                }
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            foreach (var trigger in input.Triggers)
            {
                if (!callbacks.TryGetValue(trigger.Name, out var callback))
                {
                    continue;
                }

                trigger.OnActivate -= callback;
            }

            input.Triggers.CollectionChanged -= handleCollectionChanged;

            base.Dispose(isDisposing);
        }
    }
}
