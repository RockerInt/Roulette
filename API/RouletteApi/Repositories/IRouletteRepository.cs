using RouletteApi.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RouletteApi.Repositories
{
    public interface IRouletteRepository
    {
        bool Exist(Guid key);
        List<dynamic> Get();
        dynamic GetById(Guid id);
        Guid Create();
        bool OpenBets(Guid id);
        bool MakeBet(Guid keyRoulette, Guid userId, Bet bet);
        RouletteResult CloseBets(Guid id);
    }
}