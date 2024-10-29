
using Microsoft.AspNetCore.Mvc;
using PlusAppointment.Models.Classes;

using PlusAppointment.Services.Interfaces.ShopPictureService;

namespace PlusAppointment.Controllers.ShopPictureController;

[Route("api/[controller]")]
[ApiController]
public class ShopPicturesController: ControllerBase
{
    private readonly IShopPictureService _shopPictureService;

    public ShopPicturesController(IShopPictureService shopPictureService)
    {
        _shopPictureService = shopPictureService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ShopPicture>> Get(int id)
    {
        var picture = await _shopPictureService.GetPictureAsync(id);
        if (picture == null)
        {
            return NotFound();
        }
        return Ok(picture);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ShopPicture>>> GetAll()
    {
        var pictures = await _shopPictureService.GetAllPicturesAsync();
        return Ok(pictures);
    }

    [HttpGet("business/{businessId}")]
    public async Task<ActionResult<IEnumerable<ShopPicture>>> GetByBusiness(int businessId)
    {
        var pictures = await _shopPictureService.GetPicturesByBusinessIdAsync(businessId);
        return Ok(pictures);
    }

    [HttpPost("business/{businessId}")]
    public async Task<ActionResult<ShopPicture>> Post(int businessId, IFormFile image)
    {
        var picture = await _shopPictureService.AddPictureAsync(businessId, image);
        return CreatedAtAction(nameof(Get), new { id = picture.ShopPictureId }, picture);
    }

    [HttpPost("business/{businessId}/bulk")]
    public async Task<IActionResult> PostBulk(int businessId, List<IFormFile> images)
    {
        var result = await _shopPictureService.AddPicturesAsync(businessId, images);
        if (result)
        {
            return Ok();
        }
        return BadRequest();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _shopPictureService.DeletePictureAsync(id);
        if (result)
        {
            return NoContent();
        }
        return NotFound();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, IFormFile image)
    {
        var updatedPicture = await _shopPictureService.UpdatePictureAsync(id, image);
        return Ok(updatedPicture);
    }
}