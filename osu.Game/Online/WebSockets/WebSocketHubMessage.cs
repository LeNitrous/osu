// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;

namespace osu.Game.Online.WebSockets
{
    [Serializable]
    public class WebSocketHubMessage
    {
        [JsonProperty("method")]
        public string Method { get; set; } = string.Empty;

        [JsonProperty("args")]
        public dynamic[] Args { get; set; } = Array.Empty<dynamic>();
    }
}
