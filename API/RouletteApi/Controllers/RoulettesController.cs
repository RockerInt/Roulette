using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RouletteApi.Models;
using RouletteApi.Repositories;
using Microsoft.Extensions.Logging;
using System.Linq;
using Utilities;

namespace RouletteApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class RoulettesController : ControllerBase
    {
        private readonly IRouletteRepository _repository;
        private readonly ILogger<RoulettesController> _logger;

        public RoulettesController(IRouletteRepository repository, ILogger<RoulettesController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpGet]
        [Route("Get")]
        [ProducesResponseType(typeof(List<dynamic>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetRoulettes() =>
            await Task.FromResult(
                Utilities.Utilities.TryCatch(
                    () =>
                    {
                        _logger.LogInformation("Begin HttpGet call GetRoulettes");
                        var response = _repository.Get();
                        if (!response?.Any() ?? false)
                        {
                            var message = "No results found";
                            _logger.LogInformation("HttpGet GetRoulettes response: {@response}", message);

                            return NotFound(message);
                        }
                        _logger.LogInformation("HttpGet GetRoulettes response: {@response}", response);

                        return Ok(response);
                    },
                    HttpErrorHandler
                )
            );
        [HttpGet]
        [Route("Get/{id:guid}")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetRoulette(Guid id) =>
            await Task.FromResult(
                Utilities.Utilities.TryCatch(
                    () =>
                    {
                        _logger.LogInformation("Begin HttpGet call GetRoulettes with request: {@request}", id);
                        object response = _repository.GetById(id);
                        if (response is null)
                        {
                            var message = $"The property with id {id} do not exist";
                            _logger.LogInformation("HttpGet GetRoulette response: {@response}", message);

                            return NotFound(message);
                        }
                        _logger.LogInformation("HttpGet GetRoulette response: {@response}", response);

                        return Ok(response);
                    },
                    HttpErrorHandler
                )
            );
        [HttpPost]
        [Route("Create")]
        [ProducesResponseType(typeof(Guid), (int)HttpStatusCode.Created)]
        public async Task<IActionResult> CreateRoulette() =>
            await Task.FromResult(
                Utilities.Utilities.TryCatch(
                    () =>
                    {
                        _logger.LogInformation("Begin HttpPost call CreateRoulette");
                        var response = _repository.Create();
                        _logger.LogInformation("HttpPost CreateRoulette response: {@response}", response);

                        return StatusCode((int)HttpStatusCode.Created, response);
                    },
                    HttpErrorHandler
                )
            );
        [HttpPost]
        [Route("OpenBets/{id:guid}")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> OpenBets(Guid id) =>
            await Task.FromResult(
                Utilities.Utilities.TryCatch(
                    () =>
                    {
                        _logger.LogInformation("Begin HttpPost call OpenBets with request: {@request}", id);
                        if (!_repository.Exist(id))
                        {
                            var message = $"The roulette with id {id} do not exist";
                            _logger.LogInformation("HttpPost OpenBets response: {@response}", message);

                            return NotFound(message);
                        }
                        if (_repository.GetById(id).IsOpen)
                        {
                            var message = $"The roulette with id {id} is now open";
                            _logger.LogInformation("HttpPost OpenBets response: {@response}", message);

                            return Conflict(message);
                        }
                        var response = _repository.OpenBets(id);
                        _logger.LogInformation("HttpPost OpenBets response: {@response}", response);

                        return Ok(response);
                    },
                    HttpErrorHandler
                )
            );
        [HttpPost]
        [Route("MakeBet")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> MakeBet([FromBody] MakeBet makeBet) =>
            await Task.FromResult(
                Utilities.Utilities.TryCatch(
                    () =>
                    {
                        var _userId = Request.Headers["UserId"];
                        _logger.LogInformation(
                            "Begin HttpPost call MakeBet with request: {@request}",
                            new
                            {
                                makeBet.RouletteId,
                                UserId = _userId,
                                makeBet.Bet
                            }
                        );
                        var userId = Convert.ToString(_userId);
                        if (ModelState.IsValid && !string.IsNullOrWhiteSpace(userId))
                        {
                            if (!_repository.Exist(makeBet.RouletteId))
                            {
                                var message = $"The roulette with id {makeBet.RouletteId} do not exist";
                                _logger.LogInformation("HttpPost MakeBet response: {@response}", message);

                                return NotFound(message);
                            }
                            if (!_repository.GetById(makeBet.RouletteId).IsOpen)
                            {
                                var message = $"The roulette with id {makeBet.RouletteId} is not open";
                                _logger.LogInformation("HttpPost OpenBets response: {@response}", message);

                                return Conflict(message);
                            }
                            var response = _repository.MakeBet(makeBet.RouletteId, userId.ToGuid().Value, makeBet.Bet);
                            _logger.LogInformation("HttpPost MakeBet response: {@response}", response);

                            return Ok(response);
                        }
                        else
                        {
                            _logger.LogInformation("HttpPost MakeBet response: {@response}", ModelState.SelectMany(x => x.Value.Errors));

                            return BadRequest();
                        }
                    },
                    HttpErrorHandler
                )
            );
        [HttpPost]
        [Route("CloseBets/{id:guid}")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(List<BetUser>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CloseBets(Guid id) =>
            await Task.FromResult(
                Utilities.Utilities.TryCatch(
                    () =>
                    {
                        _logger.LogInformation("Begin HttpPost call CloseBets with request: {@request}", id);
                        if (!_repository.Exist(id))
                        {
                            var message = $"The roulette with id {id} do not exist";
                            _logger.LogInformation("HttpPost CloseBets response: {@response}", message);

                            return NotFound(message);
                        }
                        if (!_repository.GetById(id).IsOpen)
                        {
                            var message = $"The roulette with id {id} is now close";
                            _logger.LogInformation("HttpPost CloseBets response: {@response}", message);

                            return Conflict(message);
                        }
                        var response = _repository.CloseBets(id);
                        _logger.LogInformation("HttpPost CloseBets response: {@response}", response);

                        return Ok(response);
                    },
                    HttpErrorHandler
                )
            );
        private IActionResult HttpErrorHandler(Exception error)
        {
            _logger.LogError("An error has occurred: @error", error);
            return StatusCode((int)HttpStatusCode.InternalServerError, "An error has occurred, contact the administrator!");
        }
    }
}
