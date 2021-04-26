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
        private static Guid? _rouletteResultsId;
        private readonly ICacheClient _cache;

        public Guid RoulettesId
        {
            get
            {
                if (!_roulettesId.HasValue) _roulettesId = Guid.NewGuid();
                return _roulettesId.Value;
            }
        }
        public Guid RouletteResultsId
        {
            get
            {
                if (!_rouletteResultsId.HasValue) _rouletteResultsId = Guid.NewGuid();
                return _rouletteResultsId.Value;
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
        public List<Guid> RouletteResults
        {
            get
            {
                if (_cache.HasValue(RouletteResultsId.ToString()))
                    return _cache.JsonGet<List<Guid>>(RouletteResultsId.ToString());
                else
                {
                    List<Guid> roulettes = new();
                    _cache.JsonSet(RouletteResultsId.ToString(), roulettes);

                    return roulettes;
                }
            }
            private set
            {
                _cache.JsonSet(RouletteResultsId.ToString(), value);
            }
        }

        public RouletteRepository(ICacheClient cache)
        {
            _cache = cache;
        }

        public bool Exist(Guid key) => _cache.HasValue(key.ToString());
        public List<dynamic> Get()
        {
            var list = new List<dynamic>();
            list.AddRange(Roulettes.Select(x => (dynamic)GetRoulette(x)));
            list.AddRange(RouletteResults.Select(x => (dynamic)GetRouletteResult(x)));

            return list;
        }
        private RouletteResult GetRouletteResult(Guid id) => _cache.JsonGet<RouletteResult>(id.ToString());
        public Roulette GetRoulette(Guid id) => _cache.JsonGet<Roulette>(id.ToString());
        public dynamic GetById(Guid id) => Roulettes.Contains(id) ? GetRoulette(id) : GetRouletteResult(id);
        public Guid Create()
        {
            var key = Guid.NewGuid();
            Roulette roulette = new() { Id = key };
            Roulettes = Roulettes.Append(key).ToList();
            _cache.JsonSet(key.ToString(), roulette);

            return key;
        }
        public bool Update(Roulette roulette) => _cache.JsonSet(roulette.Id.ToString(), roulette);
        public bool OpenBets(Guid id)
        {
            var roulette = GetRoulette(id);
            roulette.IsOpen = true;

            return Update(roulette);
        }
        public bool MakeBet(Guid rouletteId, Guid userId, Bet bet)
        {
            var roulette = GetRoulette(rouletteId);
            if (bet.Number.HasValue) bet.Color = bet.BetValue % 2 == 0 ? Color.Red : Color.Black;
            roulette.Bets.Add(new BetUser() { UserId = userId, Bet = bet });

            return Update(roulette);
        }
        public RouletteResult CloseBets(Guid id)
        {
            var roulette = GetRoulette(id);
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
            Roulettes = Roulettes.Where(x => x != id).ToList();
            RouletteResults = RouletteResults.Append(id).ToList();

            return rouletteResult;
        }
        private static RouletteResult ProcessBets(RouletteResult rouletteResult, int number, Color color)
        {
            rouletteResult.Bets.ForEach(
                x => x.BetResult = x.Bet.BetValue * (x.Bet.Number == number ? 5m : x.Bet.Color == color ? 1.8m : 0m)
            );

            return rouletteResult;
        }
        private static (int number, Color color) SpinRoulette()
        {
            var random = new Random();
            int number = random.Next(0, 37);
            Color color = number % 2 == 0 ? Color.Red : Color.Black;

            return (number, color);
        }
    }
}
