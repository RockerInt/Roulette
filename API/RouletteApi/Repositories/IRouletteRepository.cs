using RouletteApi.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RouletteApi.Repositories
{
    public interface IRouletteRepository
    {
        bool Exist(Guid key);
        List<Roulette> Get();
        Roulette GetById(Guid id);
        Guid Create();
        bool OpenBets(Guid id);
        bool MakeBet(Guid keyRoulette, Guid userId, Bet bet);
        List<BetUser> CloseBets(Guid id);
    }
}