// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Play.PlayerSettings;

namespace osu.Game.Screens.Edit.GameplayTest
{
    public class EditorHUDOverlay : HUDOverlay
    {
        [Resolved]
        private Bindable<IReadOnlyList<Mod>> mods { get; set; }

        private ModSelectOverlay modSelect;
        private GameplayClockContainer gameplayClock;

        public EditorHUDOverlay(GameplayClockContainer gameplayClock, DrawableRuleset drawableRuleset, IReadOnlyList<Mod> mods)
            : base(drawableRuleset, mods)
        {
            this.gameplayClock = gameplayClock;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            (PlayerSettingsOverlay.Child as FillFlowContainer<PlayerSettingsGroup>)?.Add(new EditorGameplaySettings
            {
                ShowModsOverlay = () =>
                {
                    gameplayClock.Stop();
                    modSelect.Show();
                }
            });

            AddInternal(modSelect = new UserModSelectOverlay
            {
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
            });

            mods.Disabled = false;

            modSelect.SelectedMods.BindTo(mods);
            modSelect.State.BindValueChanged(e =>
            {
                if (gameplayClock.IsPaused.Value && e.NewValue == Visibility.Hidden)
                    gameplayClock.Start();
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            PlayerSettingsOverlay.Show();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            mods.Disabled = true;
        }

        private class EditorGameplaySettings : PlayerSettingsGroup
        {
            public Action ShowModsOverlay;

            public EditorGameplaySettings()
                : base("Gameplay")
            {
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Children = new Drawable[]
                {
                    new TriangleButton
                    {
                        Text = "Mods",
                        Action = ShowModsOverlay,
                        RelativeSizeAxes = Axes.X,
                    },
                };
            }
        }
    }
}
