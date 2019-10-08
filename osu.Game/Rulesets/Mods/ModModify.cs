// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModModify : Mod, IApplicableToDifficulty
    {
        public override string Name => "Modify";

        public override string Acronym => "MD";

        public override string Description => "Play it your way!";

        public override double ScoreMultiplier => 1.0f;

        public override ModType Type => ModType.Conversion;

        public override IconUsage Icon => FontAwesome.Solid.Wrench;

        public override Type[] IncompatibleMods => new[] { typeof(ModHardRock), typeof(ModEasy) };

        protected readonly BindableFloat od = new BindableFloat
        {
            Precision = 0.1f,
            MinValue = 1,
            MaxValue = 10,
            Default = 5,
            Value = 5,
        };

        protected readonly BindableFloat cs = new BindableFloat
        {
            Precision = 0.1f,
            MinValue = 1,
            MaxValue = 10,
            Default = 5,
            Value = 5,
        };

        protected readonly BindableFloat hp = new BindableFloat
        {
            Precision = 0.1f,
            MinValue = 1,
            MaxValue = 10,
            Default = 5,
            Value = 5,
        };

        protected readonly BindableFloat ar = new BindableFloat
        {
            Precision = 0.1f,
            MinValue = 1,
            MaxValue = 10,
            Default = 5,
            Value = 5,
        };

        public void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            difficulty.CircleSize = cs.Value;
            difficulty.ApproachRate = ar.Value;
            difficulty.DrainRate = hp.Value;
            difficulty.OverallDifficulty = od.Value;
        }
    }
}