using Microsoft.AspNetCore.Mvc;
using PlusAppointment.Models.DTOs.DiscountCode;
using PlusAppointment.Services.Interfaces.DiscountCodeService;

namespace PlusAppointment.Controllers.DiscountCode;

[ApiController]
[Route("api/[controller]")]
public class DiscountCodeController : ControllerBase
{
    private readonly IDiscountCodeService _discountCodeService;

    public DiscountCodeController(IDiscountCodeService discountCodeService)
    {
        _discountCodeService = discountCodeService;
    }

    // POST: api/discountcode/verifyanduse
    [HttpPost("verifyanduse")]
    public async Task<IActionResult> VerifyAndUseDiscountCode([FromBody] DiscountCodeRequestDto discountCodeRequest)
    {
        var discountCode = await _discountCodeService.VerifyAndUseDiscountCodeAsync(discountCodeRequest.Code);
        if (discountCode == null)
        {
            return NotFound(new { message = "Invalid or already used discount code." });
        }

        return Ok(new { message = "Discount code is valid and now marked as used.", discountCode.DiscountPercentage });
    }
}