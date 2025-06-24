using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using ThreeTP.Payment.Application.DTOs.Requests.Pasarela;
using ThreeTP.Payment.Application.DTOs.Requests.Tenants;
using ThreeTP.Payment.Application.DTOs.Responses.Pasarela;
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Application.Interfaces.Payment;
using ThreeTP.Payment.Application.Services;
using ThreeTP.Payment.Domain.Entities.Tenant;
using ThreeTP.Payment.Infrastructure.Services.Neutrino;

namespace ThreeTP.Payment.API.Controller
{
    /// <summary>
    /// Manages payment processing and transaction queries for the NMI payment gateway.
    /// </summary>
    [ApiController]
        [Route("api/payments")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly BinLookupService _binLookupService;
        private readonly ILogger<PaymentsController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentsController"/> class.
        /// </summary>
        /// <param name="terminalService"></param>
        /// <param name="paymentService">The service for processing payments and queries.</param>
        /// <param name="binLookupService"></param>
        /// <param name="logger"></param>
        public PaymentsController(
            IPaymentService paymentService,
            BinLookupService binLookupService,
            ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _binLookupService = binLookupService;
            _logger = logger;
        }

        #region payments

        // [HttpPost]
        // public async Task<IActionResult> ProcessPayment(PaymentRequest request)
        // {
        //     var tenant = HttpContext.Items["Tenant"] as Tenant;
        //     var terminal = tenant?.Terminals.FirstOrDefault(); // O selecciona por ID
        //
        //     if (terminal == null) return Forbid();
        //
        //     var secretKey = await _terminalService.GetDecryptedSecretKeyAsync(terminal.Id);
        //     var result = await _paymentGatewayService.ProcessPaymentAsync(request, secretKey);
        //
        //     return Ok(result);
        // }


        /// <summary>
        /// Processes a payment transaction using the NMI payment gateway.
        /// </summary>
        /// <param name="apiKey">The API key for tenant authentication, provided in the request header.</param>
        /// <param name="paymentRequest">The payment request details.</param>
        /// <returns>A <see cref="NmiResponseDto"/> containing the payment response.</returns>
        /// <response code="200">Payment processed successfully.</response>
        /// <response code="400">Invalid request data.</response>
        /// <response code="401">Invalid or inactive API key.</response>
        /// <response code="500">Server error occurred.</response>
        [HttpPost]
        [ProducesResponseType(typeof(NmiResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ProcessPayment(
            [FromHeader(Name = "X-Api-Key")] [Required]
            string apiKey,
            [FromBody] BaseTransactionRequestDto paymentRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var response = await _paymentService.ProcessPaymentAsync(apiKey, paymentRequest);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"An error occurred while processing the payment.:{ex.Message}");
            }
        }

        /// <summary>
        /// Queries a transaction using the NMI payment gateway.
        /// </summary>
        /// <param name="apiKey">The API key for tenant authentication, provided in the request header.</param>
        /// <param name="queryTransactionRequest">The transaction query details.</param>
        /// <returns>A <see cref="QueryResponseDto.NmResponse"/> containing the query response.</returns>
        /// <response code="200">Query processed successfully.</response>
        /// <response code="400">Invalid request data.</response>
        /// <response code="401">Invalid or inactive API key.</response>
        /// <response code="500">Server error occurred.</response>
        [HttpPost("query")]
        [ProducesResponseType(typeof(QueryResponseDto.NmResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> QueryTransaction(
            [FromHeader(Name = "X-Api-Key")] [Required]
            string apiKey,
            [FromBody] QueryTransactionRequestDto queryTransactionRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var response = await _paymentService.QueryProcessPaymentAsync(apiKey, queryTransactionRequest);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"An error occurred while querying the transaction:{ex.Message}");
            }
        }

        [HttpGet("binlookup/{bin}")]
        public async Task<IActionResult> GetBinLookup([FromRoute] string bin)
        {
            // Validate BIN
            // if (string.IsNullOrWhiteSpace(bin) || !IsValidBin(bin))
            // {
            //     _logger.LogWarning("Invalid BIN format received: {MaskedBin}", MaskBin(bin));
            //     return BadRequest("Invalid BIN format. BIN must be 6-8 digits.");
            // }

            _logger.LogInformation("Starting bin lookup for BIN: {MaskedBin}", MaskBin(bin));

            try
            {
                var binDataDto = await _binLookupService.GetBinLookupAsync(bin);
                _logger.LogInformation("Successfully retrieved bin data for BIN: {MaskedBin}", MaskBin(bin));
                return Ok(binDataDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during bin lookup for BIN: {MaskedBin}", MaskBin(bin));
                return StatusCode(500, new { error = "An error occurred while processing the BIN lookup." });
            }
        }

        private bool IsValidBin(string bin)
        {
            // BINs are typically 6-8 digits
            return bin.Length >= 6 && bin.Length <= 8 && bin.All(char.IsDigit);
        }

        private string MaskBin(string bin)
        {
            // Mask BIN for PCI DSS compliance (e.g., show first 6 digits only)
            if (string.IsNullOrEmpty(bin)) return "******";
            return bin.Length >= 6 ? bin[..6] + "******" : new string('*', bin.Length);
        }

        #endregion
        
    }
}