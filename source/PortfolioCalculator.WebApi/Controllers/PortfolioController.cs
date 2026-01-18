using MediatR;
using Microsoft.AspNetCore.Mvc;
using PortfolioCalculator.Application.PortfolioValuation;
using PortfolioCalculator.Application.PortfolioValuation.DTOs;

namespace PortfolioCalculator.WebApi.Controllers
{
    [ApiController]
    [Route("api/portfolio")]
    public sealed class PortfolioController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PortfolioController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Returns total portfolio value and breakdown by investment type for a given investor at reference date.
        /// </summary>
        /// <param name="investorId">Investor identifier (e.g., Investor0)</param>
        /// <param name="date">Reference date (yyyy-MM-dd)</param>
        [HttpGet("value")]
        [ProducesResponseType(typeof(PortfolioValuationResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PortfolioValuationResultDto>> GetValue(
            [FromQuery] string investorId,
            [FromQuery] DateTime date,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(investorId))
                return BadRequest("investorId is required.");

            PortfolioValuationResultDto result = await _mediator.Send(
                new GetPortfolioValueQuery(investorId, date));

            return Ok(result);
        }
    }
}
