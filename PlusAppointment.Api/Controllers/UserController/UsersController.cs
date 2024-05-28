using Microsoft.AspNetCore.Mvc;
using PlusAppointment.Models.DTOs;
using WebApplication1.Models;
using WebApplication1.Services.Interfaces.UserService;

namespace WebApplication1.Controllers.UserController;

[ApiController]
[Route("api/[controller]")]
public class UsersController: ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userService.GetAllUsersAsync();
        if (users == null || !users.Any())
        {
            return NotFound(new { message = "No users found." });
        }
        return Ok(new { message = "Users retrieved successfully.", data = users });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound(new { message = $"User with ID {id} not found." });
        }
        return Ok(new { message = "User retrieved successfully.", data = user });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegisterDto userRegisterDto)
    {
        try
        {
            await _userService.RegisterUserAsync(userRegisterDto.Username, userRegisterDto.Password, userRegisterDto.Email);
            return Ok(new { message = "User registered successfully!" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Registration failed: {ex.Message}" });
        }
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var token = await _userService.LoginAsync(loginDto.UsernameOrEmail, loginDto.Password);
        if (token == null)
        {
            return Unauthorized(new { message = "Invalid credentials." });
        }

        return Ok(new { token });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] User user)
    {
        if (id != user.UserId)
        {
            return BadRequest(new { message = "User ID mismatch." });
        }

        try
        {
            await _userService.UpdateUserAsync(user);
            return Ok(new { message = "User updated successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Update failed: {ex.Message}" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _userService.DeleteUserAsync(id);
            return Ok(new { message = "User deleted successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Delete failed: {ex.Message}" });
        }
    }

}