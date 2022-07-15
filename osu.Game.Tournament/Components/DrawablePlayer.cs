// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using osu.Framework.Graphics.Containers;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Components
{
    public abstract class DrawablePlayer : CompositeDrawable
    {
        public readonly TournamentTeam Team;
        public TournamentUser User => Team?.Players.FirstOrDefault() ?? default_user;

        protected DrawablePlayer(TournamentTeam team)
        {
            Team = team;
        }

        private static readonly TournamentUser default_user = new()
        {
            Username = @"Dummy",
            CountryRank = null,
            OnlineID = -1,
        };
    }
}
