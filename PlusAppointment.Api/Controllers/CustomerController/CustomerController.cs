using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlusAppointment.Models.DTOs;
using PlusAppointment.Services.Interfaces.CustomerService;

namespace PlusAppointment.Controllers.CustomerController
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            return Ok(customers);
        }

        [HttpGet("customer_id={customerId}")]
        [Authorize]
        public async Task<IActionResult> GetById(int customerId)
        {
            var customer = await _customerService.GetCustomerByIdAsync(customerId);
            if (customer == null)
            {
                return NotFound(new { message = "Customer not found" });
            }

            return Ok(customer);
        }
        
        [HttpPost("find-customer")]
        public async Task<IActionResult> FindByEmailOrPhone([FromBody] FindCustomerDto findCustomerDto)
        {
            var customer = await _customerService.GetCustomerByEmailOrPhoneAsync(findCustomerDto.EmailOrPhone);
            if (customer == null)
            {
                return NotFound(new { message = "Customer not found. Phone or Email is not correct" });
            }

            return Ok(new { customer.CustomerId });
        }
        
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddCustomer([FromBody] CustomerDto customerDto)
        {
            try
            {
                await _customerService.AddCustomerAsync(customerDto);
                return Ok(new { message = "Customer added successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("customer_id={customerId}")]
        [Authorize]
        public async Task<IActionResult> UpdateCustomer(int customerId, [FromBody] CustomerDto? customerDto)
        {
            if (customerDto == null)
            {
                return BadRequest(new { message = "No data provided." });
            }

            try
            {
                await _customerService.UpdateCustomerAsync(customerId, customerDto);
                return Ok(new { message = "Customer updated successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Update failed: {ex.Message}" });
            }
        }

        [HttpDelete("customer_id={customerId}")]
        [Authorize]
        public async Task<IActionResult> DeleteCustomer(int customerId)
        {
            try
            {
                await _customerService.DeleteCustomerAsync(customerId);
                return Ok(new { message = "Customer deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
