using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs;
using PlusAppointment.Services.Interfaces.ServiceCategoryService;

namespace PlusAppointment.Controllers.ServiceCategoryController
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceCategoryController : ControllerBase
    {
        private readonly IServiceCategoryService _serviceCategoryService;

        public ServiceCategoryController(IServiceCategoryService serviceCategoryService)
        {
            _serviceCategoryService = serviceCategoryService;
        }

        [AllowAnonymous]
        [HttpGet("category_id={categoryId}")]
        public async Task<IActionResult> GetById(int categoryId)
        {
            var serviceCategory = await _serviceCategoryService.GetServiceCategoryByIdAsync(categoryId);
            if (serviceCategory == null)
            {
                return NotFound(new { message = "Service category not found" });
            }

            return Ok(serviceCategory);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var serviceCategories = await _serviceCategoryService.GetAllServiceCategoriesAsync();
            return Ok(serviceCategories);
        }

        [Authorize(Roles = "Admin,Owner")]
        [HttpPost]
        public async Task<IActionResult> AddServiceCategory([FromBody] ServiceCategoryDto serviceCategoryDto)
        {
            if (serviceCategoryDto == null || string.IsNullOrEmpty(serviceCategoryDto.Name))
            {
                return BadRequest(new { message = "Invalid service category data" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authorized" });
            }

            try
            {
                var serviceCategory = new ServiceCategory
                {
                    Name = serviceCategoryDto.Name,
                    Color = serviceCategoryDto.Color // Add color property
                };
                await _serviceCategoryService.AddServiceCategoryAsync(serviceCategory);
                return Ok(new { message = "Service category created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Creation failed: {ex.Message}" });
            }
        }

        [Authorize(Roles = "Admin,Owner")]
        [HttpPut("category_id={categoryId}")]
        public async Task<IActionResult> UpdateServiceCategory(int categoryId,
            [FromBody] ServiceCategoryDto serviceCategoryDto)
        {
            if (serviceCategoryDto == null || string.IsNullOrEmpty(serviceCategoryDto.Name))
            {
                return BadRequest(new { message = "Invalid service category data" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authorized" });
            }

            try
            {
                var serviceCategory = new ServiceCategory
                {
                    CategoryId = categoryId,
                    Name = serviceCategoryDto.Name,
                    Color = serviceCategoryDto.Color // Include color in update
                };
                await _serviceCategoryService.UpdateServiceCategoryAsync(serviceCategory);
                return Ok(new { message = "Service category updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Update failed: {ex.Message}" });
            }
        }

        [Authorize(Roles = "Admin,Owner")]
        [HttpDelete("category_id={categoryId}")]
        public async Task<IActionResult> DeleteServiceCategory(int categoryId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authorized" });
            }

            try
            {
                await _serviceCategoryService.DeleteServiceCategoryAsync(categoryId);
                return Ok(new { message = "Service category deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Delete failed: {ex.Message}" });
            }
        }
    }
}