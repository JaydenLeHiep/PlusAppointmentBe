using Microsoft.AspNetCore.Mvc;
using PlusAppointment.Models.Classes.CheckIn;
using PlusAppointment.Models.DTOs.CheckIn;
using PlusAppointment.Services.Interfaces.DiscountTierService;

namespace PlusAppointment.Controllers.DiscountTierController;

[ApiController]
[Route("api/[controller]")]
public class DiscountTierController : ControllerBase
{
    private readonly IDiscountTierService _discountTierService;

    public DiscountTierController(IDiscountTierService discountTierService)
    {
        _discountTierService = discountTierService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var discountTier = await _discountTierService.GetDiscountTierByIdAsync(id);
        if (discountTier == null)
        {
            return NotFound(new { message = "Discount tier not found." });
        }

        return Ok(discountTier);
    }

    [HttpGet("business/{businessId}")]
    public async Task<IActionResult> GetByBusinessId(int businessId)
    {
        var discountTiers = await _discountTierService.GetDiscountTiersByBusinessIdAsync(businessId);
        return Ok(discountTiers);
    }

    [HttpPost("business/{businessId}")]
    public async Task<IActionResult> AddDiscountTier(int businessId, [FromBody] DiscountTierDto discountTierDto)
    {
        
        try
        {
            await _discountTierService.AddDiscountTierAsync(discountTierDto, businessId);
            return Ok(new { message = "Discount tier added successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] DiscountTierDto discountTierDto)
    {
        if (discountTierDto == null)
        {
            return BadRequest(new { message = "Discount tier data cannot be null." });
        }

        await _discountTierService.UpdateDiscountTierAsync(id, discountTierDto);
        return Ok(new { message = "Discount tier updated successfully." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _discountTierService.DeleteDiscountTierAsync(id);
        return Ok(new { message = "Discount tier deleted successfully." });
    }
}