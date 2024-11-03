using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PlusAppointment.Services.Interfaces.CustomerService;
using PlusAppointment.Models.Classes;
using PlusAppointment.Models.DTOs.Customers;
using PlusAppointment.Utils.Hub;

namespace PlusAppointment.Controllers.CustomerController
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly IHubContext<AppointmentHub> _hubContext;

        public CustomerController(ICustomerService customerService, IHubContext<AppointmentHub> hubContext)
        {
            _customerService = customerService;
            _hubContext = hubContext;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            return Ok(customers);
        }
        
        [AllowAnonymous]
        [HttpGet("{customerId}")]
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
        [HttpGet("business/{businessId}")]
        [Authorize]
        public async Task<IActionResult> GetCustomersByBusinessId(int businessId)
        {
            var customers = await _customerService.GetCustomersByBusinessIdAsync(businessId);
            

            return Ok(customers);
        }
        
        [HttpPost("find-customer")]
        public async Task<IActionResult> FindByEmailOrPhone([FromBody] FindCustomerDto findCustomerDto)
        {
            var customer = await _customerService.GetCustomerByEmailOrPhoneAsync(findCustomerDto.EmailOrPhone);
    
            if (customer == null)
            {
                return Ok(new { message = "Customer not found.", customerExists = false });
            }

            return Ok(new { customer.CustomerId, customerExists = true });
        }
        
        [AllowAnonymous]
        [HttpPost("find-customer/business/{businessId}")]
        public async Task<IActionResult> FindByEmailOrPhone(int businessId, [FromBody] FindCustomerDto findCustomerDto)
        {
            var customer = await _customerService.GetCustomerByEmailOrPhoneAndBusinessIdAsync(findCustomerDto.EmailOrPhone, businessId);

            if (customer == null)
            {
                return Ok(new { message = "Customer not found.", customerExists = false });
            }

            return Ok(new { customer.CustomerId, customer.Name, customerExists = true });
        }

        
        
        [HttpGet("{customerId}/find-appointments")]
        [Authorize]
        public async Task<IActionResult> GetCustomerAppointments(int customerId)
        {
            var appointments = await _customerService.GetCustomerAppointmentsAsync(customerId);
            if (!appointments.Any())
            {
                return NotFound(new { message = "No appointments found for this customer" });
            }

            return Ok(appointments);
        }
        
        [HttpPost("business/{businessId}")]
        public async Task<IActionResult> AddCustomer(int businessId, [FromBody] CustomerDto customerDto)
        {
            try
            {
                // Assign the business_id from the URL to the DTO
                await _customerService.AddCustomerAsync(businessId, customerDto);
                // Notify the frontend
                await _hubContext.Clients.All.SendAsync("ReceiveCustomerUpdate", "A new customer has been added.");
                return Ok(new { message = "Customer added successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{customerId}")]
        
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

        [HttpDelete("{customerId}")]
        
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
        
        [HttpGet("search")]
        public async Task<IActionResult> SearchByNameOrPhone([FromQuery(Name = "name")] string searchTerm)
        {
            var customers = await _customerService.SearchCustomersByNameOrPhoneAsync(searchTerm);

            if (!customers.Any())
            {
                return Ok(new 
                { 
                    message = "No customers found with the given search term.",
                    customers = new List<Customer>() // Return an empty list for consistency
                });
            }

            return Ok(new 
            { 
                message = "Customers found.",
                customers = customers
            });
        }
        
        [AllowAnonymous]
        [HttpGet("find-customer-by-name-or-phone")]
        public async Task<IActionResult> FindByNameOrPhone([FromQuery] string nameOrPhone)
        {
            var customer = await _customerService.GetCustomerByNameOrPhoneAsync(nameOrPhone);
            if (customer == null)
            {
                return NotFound(new { message = "Customer not found. Name or Phone is not correct" });
            }

            return Ok(new { customer.CustomerId });
        }
    }
}
