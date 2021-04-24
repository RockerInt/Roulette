using RouletteApi.Models.Enums;
using System;
using System.Collections.Generic;

namespace RouletteApi.Models
{
    [Serializable]
    public class RouletteResult : RouletteBase
    {
        public RouletteResult()
        {
            Bets = new List<BetUser>();
        }

        public nint Number { get; set; }
        public Color Color { get; set; }
    }
}
