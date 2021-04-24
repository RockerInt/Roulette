using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RouletteApi.Controllers;
using RouletteApi.Models;
using RouletteApi.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities;

namespace Roulettes.UnitTest
{
    public class RoulettesControllerTests
    {
        private readonly Mock<IRouletteRepository> _rouletteRepositoryMock;
        private readonly Mock<ILogger<RoulettesController>> _loggerMock;
        private RoulettesController RouletteController;
        private Guid FakeRouletteId;
        private Guid FakeNotFoundRouletteId;
        private Guid FakeOpenRouletteId;
        private Guid FakeCloseRouletteId;
        private Guid FakeBetId;
        private Roulette FakeRoulette;
        private Roulette FakeOpenRoulette;
        private Roulette FakeCloseRoulette;
        private BetUser FakeBet;

        public RoulettesControllerTests()
        {
            _rouletteRepositoryMock = new Mock<IRouletteRepository>();
            _loggerMock = new Mock<ILogger<RoulettesController>>();
        }

        [SetUp]
        public void Setup()
        {
            FakeRouletteId = Guid.Parse("8cc32b40-578d-47c1-bb9f-63240737243f");
            FakeNotFoundRouletteId = Guid.Parse("00000000-0000-0000-0000-000000000000");
            FakeOpenRouletteId = Guid.Parse("b5b1a0c6-efc7-43b4-91b1-024a0268a7cf");
            FakeCloseRouletteId = Guid.Parse("90e031e0-9db9-4aa2-af94-832c6e13eed4");
            FakeBetId = Guid.Parse("496929ae-c452-41ea-8b45-67e179b75f36");
            FakeRoulette = GetRouletteFake(FakeRouletteId, false);
            FakeOpenRoulette = GetRouletteFake(FakeOpenRouletteId, true);
            FakeCloseRoulette = GetRouletteFake(FakeCloseRouletteId, false);
            FakeBet = GetBetFake(FakeBetId, 4, 5000m);

            _rouletteRepositoryMock.Setup(x => x.GetById(It.Is<Guid>(x => x == FakeRouletteId)))
                .Returns(FakeRoulette);
            _rouletteRepositoryMock.Setup(x => x.GetById(It.Is<Guid>(x => x == FakeOpenRouletteId)))
                .Returns(FakeOpenRoulette);
            _rouletteRepositoryMock.Setup(x => x.Get())
                .Returns(new List<Roulette>() { FakeRoulette });
            _rouletteRepositoryMock.Setup(x => x.Create())
                .Returns(FakeRouletteId);
            _rouletteRepositoryMock.Setup(x => x.OpenBets(It.Is<Guid>(x => x == FakeRouletteId)))
                .Returns(true);
            _rouletteRepositoryMock.Setup(x => x.CloseBets(It.Is<Guid>(x => x == FakeOpenRouletteId)))
                .Returns(new List<BetUser>() { FakeBet });
            _rouletteRepositoryMock.Setup(x => x.Exist(It.Is<Guid>(x => x == FakeRouletteId)))
                .Returns(true);
            _rouletteRepositoryMock.Setup(
                x => x.MakeBet(
                    It.Is<Guid>(x => x == FakeOpenRouletteId),
                    It.Is<Guid>(x => x == FakeBetId),
                    FakeBet.Bet
                )).Returns(true);
            
            Roulette nullObj = null;
            _rouletteRepositoryMock.Setup(x => x.GetById(It.Is<Guid>(x => x == FakeNotFoundRouletteId)))
                .Returns(nullObj);
            _rouletteRepositoryMock.Setup(x => x.GetById(It.Is<Guid>(x => x == FakeOpenRouletteId)))
                .Returns(FakeOpenRoulette);
            _rouletteRepositoryMock.Setup(x => x.GetById(It.Is<Guid>(x => x == FakeCloseRouletteId)))
                .Returns(FakeCloseRoulette);
            _rouletteRepositoryMock.Setup(x => x.Exist(It.Is<Guid>(x => x == FakeNotFoundRouletteId)))
                .Returns(false);
            _rouletteRepositoryMock.Setup(x => x.Exist(It.Is<Guid>(x => x == FakeOpenRouletteId)))
                .Returns(true);
            _rouletteRepositoryMock.Setup(x => x.Exist(It.Is<Guid>(x => x == FakeCloseRouletteId)))
                .Returns(true);

            RouletteController = new RoulettesController(
                _rouletteRepositoryMock.Object,
                _loggerMock.Object
            );
        }

        #region Success Cases
        [Test]
        public void GetRouletteTest()
        {
            var actionResult = RouletteController.GetRoulette(FakeRouletteId).Result;
            Assert.AreEqual(((ObjectResult)actionResult).StatusCode, (int)System.Net.HttpStatusCode.OK);
            Assert.AreEqual(((Roulette)((ObjectResult)actionResult).Value).Id, FakeRouletteId);
        }
        [Test]
        public void GetRoulettesTest()
        {
            var actionResult = RouletteController.GetRoulettes().Result;
            Assert.AreEqual(((ObjectResult)actionResult).StatusCode, (int)System.Net.HttpStatusCode.OK);
            Assert.AreEqual(((List<Roulette>)((ObjectResult)actionResult).Value).FirstOrDefault().Id, FakeRouletteId);
        }
        [Test]
        public void CreateTest()
        {
            var actionResult = RouletteController.CreateRoulette().Result;
            Assert.AreEqual(((ObjectResult)actionResult).StatusCode, (int)System.Net.HttpStatusCode.Created);
            Assert.AreEqual(((ObjectResult)actionResult).Value, FakeRouletteId);
        }
        [Test]
        public void OpenBetsTest()
        {
            var actionResult = RouletteController.OpenBets(FakeRouletteId).Result;
            Assert.AreEqual(((ObjectResult)actionResult).StatusCode, (int)System.Net.HttpStatusCode.OK);
            Assert.AreEqual(((ObjectResult)actionResult).Value, true);
        }
        [Test]
        public void MakeBetTest()
        {
            var actionResult = RouletteController.MakeBet(FakeBetId, FakeOpenRouletteId, FakeBet.Bet).Result;
            Assert.AreEqual(((ObjectResult)actionResult).StatusCode, (int)System.Net.HttpStatusCode.OK);
            Assert.AreEqual(((ObjectResult)actionResult).Value, true);
        }
        [Test]
        public void CloseBetsTest()
        {
            var actionResult = RouletteController.CloseBets(FakeOpenRouletteId).Result;
            Assert.AreEqual(((ObjectResult)actionResult).StatusCode, (int)System.Net.HttpStatusCode.OK);
            Assert.AreEqual(((List<BetUser>)((ObjectResult)actionResult).Value).FirstOrDefault().UserId, FakeBetId);
        }
        #endregion

        #region Alternative Cases
        [Test]
        public void GetRouletteNotFoundTest()
        {
            var actionResult = RouletteController.GetRoulette(FakeNotFoundRouletteId).Result;
            Assert.AreEqual(((ObjectResult)actionResult).StatusCode, (int)System.Net.HttpStatusCode.NotFound);
        }
        [Test]
        public void OpenBetsNotFoundTest()
        {
            var actionResult = RouletteController.OpenBets(FakeNotFoundRouletteId).Result;
            Assert.AreEqual(((ObjectResult)actionResult).StatusCode, (int)System.Net.HttpStatusCode.NotFound);
        }
        [Test]
        public void OpenBetsOpenTest()
        {
            var actionResult = RouletteController.OpenBets(FakeOpenRouletteId).Result;
            Assert.AreEqual(((ConflictObjectResult)actionResult).StatusCode, (int)System.Net.HttpStatusCode.Conflict);
        }
        [Test]
        public void MakeBetNotFoundTest()
        {
            var actionResult = RouletteController.MakeBet(FakeBetId, FakeNotFoundRouletteId, FakeBet.Bet).Result;
            Assert.AreEqual(((NotFoundObjectResult)actionResult).StatusCode, (int)System.Net.HttpStatusCode.NotFound);
        }
        [Test]
        public void MakeBetCloseTest()
        {
            var actionResult = RouletteController.MakeBet(FakeBetId, FakeCloseRouletteId, FakeBet.Bet).Result;
            Assert.AreEqual(((ConflictObjectResult)actionResult).StatusCode, (int)System.Net.HttpStatusCode.Conflict);
        }
        [Test]
        public void CloseBetsNotFoundTest()
        {
            var actionResult = RouletteController.CloseBets(FakeNotFoundRouletteId).Result;
            Assert.AreEqual(((ObjectResult)actionResult).StatusCode, (int)System.Net.HttpStatusCode.NotFound);
        }
        [Test]
        public void CloseBetsCloseTest()
        {
            var actionResult = RouletteController.CloseBets(FakeCloseRouletteId).Result;
            Assert.AreEqual(((ConflictObjectResult)actionResult).StatusCode, (int)System.Net.HttpStatusCode.Conflict);
        }
        #endregion

        private static Roulette GetRouletteFake(Guid fakeRouletteId, bool isOpen) => new() { Id = fakeRouletteId, IsOpen = isOpen };
        private static BetUser GetBetFake(Guid fakeUserId, uint? number, decimal betValue)
            => new() 
            {
                UserId = fakeUserId,
                Bet = new()
                {
                    Number = number,
                    BetValue = betValue
                }
            };
    }
}