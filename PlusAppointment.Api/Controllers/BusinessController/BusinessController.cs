using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlusAppointment.Models.Classes.Business;
using PlusAppointment.Models.DTOs.Businesses;
using PlusAppointment.Services.Interfaces.BusinessService;

namespace PlusAppointment.Controllers.BusinessController
{
    [ApiController]
    [Route("api/businesses")]
    public class BusinessController : ControllerBase
    {
        private readonly IBusinessService _businessService;

        public BusinessController(IBusinessService businessService)
        {
            _businessService = businessService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllAdmin()
        {
            var businesses = await _businessService.GetAllBusinessesAsync();
            if (!businesses.Any())
            {
                return NotFound(new { message = "No businesses found." });
            }
            return Ok(businesses);
        }

        [Authorize(Roles = "Admin,Owner")]
        [HttpGet("byUser")]
        public async Task<IActionResult> GetAllByUser()
        {
            var currentUserId = GetCurrentUserId();
            var businesses = await _businessService.GetAllBusinessesByUserIdAsync(currentUserId);
            return Ok(businesses);
        }

        [HttpGet("{businessId}")]
        public async Task<IActionResult> GetById(int businessId)
        {
            var business = await _businessService.GetBusinessByIdAsync(businessId);
            if (business == null)
            {
                return NotFound(new { error = "NotFound", message = $"Business with ID {businessId} was not found." });
            }
            return Ok(business);
        }

        [HttpGet("{businessName}/booking")]
        public async Task<IActionResult> GetBusinessByName(string businessName)
        {
            var business = await _businessService.GetBusinessByNameAsync(businessName);
            if (business == null)
            {
                return NotFound(new { error = "NotFound", message = "Business not found." });
            }
            return Ok(business);
        }

        [Authorize(Roles = "Admin,Owner")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BusinessDto businessDto)
        {
            var userId = GetCurrentUserId();

            var business = new Business
            (
                name: businessDto.Name,
                address: businessDto.Address,
                phone: businessDto.Phone,
                email: businessDto.Email,
                userID: userId
            );

            await _businessService.AddBusinessAsync(business);

            return CreatedAtAction(nameof(GetById), new { businessId = business.BusinessId }, 
                new { message = "Business created successfully.", businessId = business.BusinessId });
        }


        [Authorize(Roles = "Admin,Owner")]
        [HttpPut("{businessId}")]
        public async Task<IActionResult> Update(int businessId, [FromBody] BusinessDto? businessDto)
        {
            if (businessDto == null)
            {
                return BadRequest(new { error = "Validation Error", message = "No data provided." });
            }
            

            var business = await _businessService.GetBusinessByIdAsync(businessId);
            if (business == null)
            {
                return NotFound(new { error = "NotFound", message = "Business not found." });
            }

            // Update only if fields are provided
            if (!string.IsNullOrEmpty(businessDto.Name))
            {
                business.Name = businessDto.Name;
            }

            if (!string.IsNullOrEmpty(businessDto.Address))
            {
                business.Address = businessDto.Address;
            }

            if (!string.IsNullOrEmpty(businessDto.Phone))
            {
                business.Phone = businessDto.Phone;
            }

            if (!string.IsNullOrEmpty(businessDto.Email))
            {
                business.Email = businessDto.Email;
            }

            await _businessService.UpdateBusinessAsync(businessId,business);
            return Ok(new { message = "Business updated successfully." });
        }

        [Authorize(Roles = "Admin,Owner")]
        [HttpDelete("{businessId}")]
        public async Task<IActionResult> Delete(int businessId)
        {

            await _businessService.DeleteBusinessAsync(businessId);
            return Ok(new { message = "Business deleted successfully." });
        }

        [Authorize(Roles = "Admin,Owner")]
        [HttpGet("{businessId}/services")]
        public async Task<IActionResult> GetServices(int businessId)
        {
            var services = await _businessService.GetServicesByBusinessIdAsync(businessId);
            return Ok(services);
        }

        [Authorize(Roles = "Admin,Owner")]
        [HttpGet("{businessId}/staffs")]
        public async Task<IActionResult> GetStaff(int businessId)
        {
            var staff = await _businessService.GetStaffByBusinessIdAsync(businessId);
            return Ok(staff);
        }

        private int GetCurrentUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                throw new UnauthorizedAccessException("User not authorized.");
            }
            return userId;
        }
        
    }
}
