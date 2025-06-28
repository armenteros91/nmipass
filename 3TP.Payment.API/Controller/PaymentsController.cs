using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThreeTP.Payment.Application.Commands.Payments;
using ThreeTP.Payment.Application.DTOs.Requests.Pasarela;
using ThreeTP.Payment.Application.DTOs.Responses.BIN_Checker;
using ThreeTP.Payment.Application.DTOs.Responses.Pasarela;
using ThreeTP.Payment.Application.Queries.Payments;

namespace ThreeTP.Payment.API.Controller
{
    /// <summary>
    /// Manages payment processing and transaction queries for the NMI payment gateway.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/payments")]
    public class PaymentsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PaymentsController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentsController"/> class.
        /// </summary>
        /// <param name="mediator">The mediator for sending commands and queries.</param>
        /// <param name="logger">The logger for logging messages.</param>
        public PaymentsController(
            IMediator mediator,
            ILogger<PaymentsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Processes a payment transaction using the NMI payment gateway.
        /// </summary>
        /// <param name="apiKey">The API key for tenant authentication, provided in the request header.</param>
        /// <param name="saleTransactionRequest">The payment request details.</param>
        /// <returns>A <see cref="NmiResponseDto"/> containing the payment Response.</returns>
        /// <Response code="200">Payment processed successfully.</Response>
        /// <Response code="400">Invalid request data.</Response>
        /// <Response code="401">Invalid or inactive API key.</Response>
        /// <Response code="500">Server error occurred.</Response>
        [HttpPost]
        [ProducesResponseType(typeof(NmiResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ProcessPayment(
            [FromHeader(Name = "X-Api-Key")] [Required] string apiKey,
            [FromBody] SaleTransactionRequestDto saleTransactionRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var command = new ProcessPaymentCommand(apiKey, saleTransactionRequest);
                var response = await _mediator.Send(command);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"An error occurred while processing the payment: {ex.Message}");
            }
        }

        /// <summary>
        /// Queries a transaction using the NMI payment gateway.
        /// </summary>
        /// <param name="apiKey">The API key for tenant authentication, provided in the request header.</param>
        /// <param name="queryTransactionRequest">The transaction query details.</param>
        /// <returns>A <see cref="QueryResponseDto"/> containing the query Response.</returns>
        /// <Response code="200">Query processed successfully.</Response>
        /// <Response code="400">Invalid request data.</Response>
        /// <Response code="401">Invalid or inactive API key.</Response>
        /// <Response code="500">Server error occurred.</Response>
        [HttpPost("query")]
        [ProducesResponseType(typeof(QueryResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> QueryTransaction(
            [FromHeader(Name = "X-Api-Key")] [Required] string apiKey,
            [FromBody] QueryTransactionRequestDto queryTransactionRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var query = new QueryTransactionQuery(apiKey, queryTransactionRequest);
                var response = await _mediator.Send(query);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying transaction.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"An error occurred while querying the transaction: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves BIN (Bank Identification Number) information.
        /// </summary>
        /// <param name="bin">The BIN to look up.</param>
        /// <returns>A <see cref="BinlookupResponse"/> containing BIN information.</returns>
        /// <Response code="200">BIN information retrieved successfully.</Response>
        /// <Response code="400">Invalid BIN format.</Response>
        /// <Response code="500">Server error occurred.</Response>
        [HttpGet("binlookup/{bin}")]
        [ProducesResponseType(typeof(BinlookupResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBinLookup([FromRoute] string bin)
        {
            if (string.IsNullOrWhiteSpace(bin) || !IsValidBin(bin))
            {
                _logger.LogWarning("Invalid BIN format received: {MaskedBin}", MaskBin(bin));
                return BadRequest("Invalid BIN format. BIN must be 6-8 digits.");
            }

            _logger.LogInformation("Starting bin lookup for BIN: {MaskedBin}", MaskBin(bin));

            try
            {
                var query = new GetBinLookupQuery(bin);
                var binDataDto = await _mediator.Send(query);
                _logger.LogInformation("Successfully retrieved bin data for BIN: {MaskedBin}", MaskBin(bin));
                return Ok(binDataDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during bin lookup for BIN: {MaskedBin}", MaskBin(bin));
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred while processing the BIN lookup." });
            }
        }

        private bool IsValidBin(string bin)
        {
            return bin.Length >= 6 && bin.Length <= 8 && bin.All(char.IsDigit);
        }

        private string MaskBin(string bin)
        {
            if (string.IsNullOrEmpty(bin)) return "******";
            return bin.Length >= 6 ? bin[..6] + new string('*', Math.Max(0, bin.Length - 6)) : new string('*', bin.Length);
        }
    }
}