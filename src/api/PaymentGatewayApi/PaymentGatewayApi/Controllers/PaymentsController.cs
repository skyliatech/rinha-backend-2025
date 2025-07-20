using Microsoft.AspNetCore.Mvc;
using PaymentGatewayApi.DataTransferObjects.Requests;
using PaymentGatewayApi.DataTransferObjects.Responses;
using PaymentGatewayApi.Services;

namespace PaymentGatewayApi.Controllers;

[ApiController]
[Route("[controller]")]
public class PaymentsController: ControllerBase
{
    private readonly IPaymentGatewayService _paymentGatewayService;
    public PaymentsController(IPaymentGatewayService paymentGatewayService)
    {
        _paymentGatewayService = paymentGatewayService;
    }

    [HttpPost]
    public async Task<IActionResult> SendPaymentAsync([FromBody] PaymentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var result = await _paymentGatewayService.SendPaymentAsync(request);

        if (result)
        {
            return Created();
        }
        
        return StatusCode(500, "An error occurred while processing the payment.");
    }

    [HttpGet("payments-summary")]
    public async Task<IActionResult> GetPaymentsSummaryAsync([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
       PaymentsSummaryResponse paymentsSummaryResponse = await _paymentGatewayService.GetPaymentsSummaryAsync(from, to);

       return Ok(paymentsSummaryResponse);
    }
}