using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlusAppointment.Models.DTOs;
using PlusAppointment.Models.Enums;
using WebApplication1.Services.Interfaces.UserService;
using WebApplication1.Utils.Hash;

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
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        var userRole = HttpContext.Items["UserRole"]?.ToString();
        if (userRole != Role.Admin.ToString())
        {
            return NotFound(new { message = "You are not authorized to view all users." });
        }
        var users = await _userService.GetAllUsersAsync();
        var enumerable = users.ToList();
        if (!enumerable.Any())
        {
            return NotFound(new { message = "No users found." });
        }
        return Ok(new { message = "Users retrieved successfully.", data = enumerable });
    }

    [HttpGet("user_id={id}")]
    [Authorize]
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
            if (string.IsNullOrEmpty(userRegisterDto.Username) || string.IsNullOrEmpty(userRegisterDto.Password)|| string.IsNullOrEmpty(userRegisterDto.Email)|| string.IsNullOrEmpty(userRegisterDto.Phone))
            {
                return BadRequest(new { message = "Username and Password cannot be null or empty." });
            }
            await _userService.RegisterUserAsync(userRegisterDto.Username, userRegisterDto.Password, userRegisterDto.Email, userRegisterDto.Phone);
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
        if (string.IsNullOrEmpty(loginDto.UsernameOrEmail) || string.IsNullOrEmpty(loginDto.Password))
        {
            return BadRequest(new { message = "Username or Email and Password are required." });
        }

        var (token, user) = await _userService.LoginAsync(loginDto.UsernameOrEmail, loginDto.Password);

        if (token == null || user == null)
        {
            return Unauthorized(new { message = "Invalid credentials." });
        }

        var response = new LoginResponseDto
        {
            Token = token,
            
            Username = user.Username,
            Role = user.Role.ToString()  // Convert Role enum to string
        };

        return Ok(response);
    }



    // Use for change the whole user or change only one thing like Password***
    [HttpPut("user_id={id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] UserUpdateDto? userUpdateDto)
    {
        if (userUpdateDto == null)
        {
            return BadRequest(new { message = "No data provided." });
        }

        var existingUser = await _userService.GetUserByIdAsync(id);
        if (existingUser == null)
        {
            return NotFound(new { message = "User not found." });
        }

        if (!string.IsNullOrEmpty(userUpdateDto.Username))
        {
            existingUser.Username = userUpdateDto.Username;
        }

        if (!string.IsNullOrEmpty(userUpdateDto.Password))
        {
            existingUser.Password = HashUtility.HashPassword(userUpdateDto.Password); // Ensure you hash the password
        }

        if (!string.IsNullOrEmpty(userUpdateDto.Email))
        {
            existingUser.Email = userUpdateDto.Email;
        }
        if (!string.IsNullOrEmpty(userUpdateDto.Phone))
        {
            existingUser.Phone = userUpdateDto.Phone;
        }

        try
        {
            await _userService.UpdateUserAsync(existingUser);
            return Ok(new { message = "User updated successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Update failed: {ex.Message}" });
        }
    }

    [HttpDelete("user_id={id}")]
    [Authorize]
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