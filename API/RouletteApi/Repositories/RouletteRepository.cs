using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using Microsoft.Extensions.Logging;
using RouletteApi.Models;
using RouletteApi.Persistence;
using RouletteApi.Models.Enums;

namespace RouletteApi.Repositories
{
    public class RouletteRepository : IRouletteRepository
    {
        private static Guid? _roulettesId;
        private readonly ICacheClient _cache;

        public Guid RoulettesId
        {
            get
            {
                if (!_roulettesId.HasValue) _roulettesId = Guid.NewGuid();
                return _roulettesId.Value;
            }
        }
        public List<Guid> Roulettes
        {
            get
            {
                if (_cache.HasValue(RoulettesId.ToString()))
                    return _cache.JsonGet<List<Guid>>(RoulettesId.ToString());
                else
                {
                    List<Guid> roulettes = new();
                    _cache.JsonSet(RoulettesId.ToString(), roulettes);

                    return roulettes;
                }
            }
            private set
            {
                _cache.JsonSet(RoulettesId.ToString(), value);
            }
        }

        public RouletteRepository(ICacheClient cache)
        {
            _cache = cache;
        }

        public bool Exist(Guid key) => _cache.HasValue(key.ToString());
        public List<RouletteBase> Get() => Roulettes.Select(x => Get(x)).ToList();
        private RouletteBase Get(Guid id) => _cache.JsonGet<RouletteBase>(id.ToString());
        public Roulette GetById(Guid id) => _cache.JsonGet<Roulette>(id.ToString());
        public Guid Create()
        {
            var key = Guid.NewGuid();
            Roulette roulette = new() { Id = key };
            Roulettes.Add(key);
            _cache.JsonSet(key.ToString(), roulette);

            return key;
        }
        public bool Update(Roulette roulette) => _cache.JsonSet(roulette.Id.ToString(), roulette);
        public bool OpenBets(Guid id)
        {
            var roulette = GetById(id);
            roulette.IsOpen = true;

            return Update(roulette);
        }
        public bool MakeBet(Guid rouletteId, Guid userId, Bet bet)
        {
            var roulette = GetById(rouletteId);
            if (bet.Number.HasValue) bet.Color = bet.BetValue % 2 == 0 ? Color.Red : Color.Black;
            roulette.Bets.Add(new BetUser() { UserId = userId, Bet = bet });

            return Update(roulette);
        }
        public RouletteResult CloseBets(Guid id)
        {
            var roulette = GetById(id);
            var (number, color) = SpinRoulette();
            var rouletteResult = new RouletteResult()
            {
                Id = roulette.Id,
                Number = number,
                Color = color,
                Bets = roulette.Bets
            };
            rouletteResult = ProcessBets(rouletteResult, number, color);
            _cache.JsonSet(id.ToString(), rouletteResult);

            return rouletteResult;
        }
        private static RouletteResult ProcessBets(RouletteResult rouletteResult, nint number, Color color)
        {
            rouletteResult.Bets.ForEach(
                x => x.BetResult = x.Bet.BetValue * (x.Bet.Number == number ? 5m : x.Bet.Color == color ? 1.8m : 0m)
            );

            return rouletteResult;
        }
        private static (nint number, Color color) SpinRoulette()
        {
            var random = new Random();
            nint number = random.Next(0, 37);
            Color color = number % 2 == 0 ? Color.Red : Color.Black;

            return (number, color);
        }
    }
}
