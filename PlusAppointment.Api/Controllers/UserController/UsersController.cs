using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using PlusAppointment.Models.DTOs;
using PlusAppointment.Models.Enums;
using PlusAppointment.Services.Interfaces.UserService;

using PlusAppointment.Models.Classes;

namespace PlusAppointment.Controllers.UserController
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
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

        [HttpGet("user_id={userId}")]
        [Authorize]
        public async Task<IActionResult> GetById(int userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = $"User with ID {userId} not found." });
            }
            return Ok(new { message = "User retrieved successfully.", data = user });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto userRegisterDto)
        {
            try
            {
                await _userService.RegisterUserAsync(userRegisterDto);
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
            var (token, refreshToken, user) = await _userService.LoginAsync(loginDto);

            if (token == null || user == null)
            {
                return Unauthorized(new { message = "Invalid credentials." });
            }

            var response = new LoginResponseDto
            {
                Token = token,
                Username = user.Username,
                Role = user.Role.ToString()
            };

            // Setting the refresh token as HTTP-only cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // Change to true in production (HTTPS)
                SameSite = SameSiteMode.Strict, // Required for cross-site cookies
                Expires = DateTime.UtcNow.AddDays(30)
            };
            if (refreshToken != null)
            {
                Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
                Console.WriteLine(
                    $"Set-Cookie: refreshToken={refreshToken}; Expires={cookieOptions.Expires}; Path={cookieOptions.Path}; HttpOnly={cookieOptions.HttpOnly}; Secure={cookieOptions.Secure}; SameSite={cookieOptions.SameSite}");
            }

            return Ok(response);
        }

        [HttpPost("refresh")]
        [EnableCors("AllowFrontendOnly")]
        public async Task<IActionResult> Refresh([FromBody] TokenModel tokenModel)
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized();
            }

            var (newAccessToken, newRefreshToken) = await _userService.RefreshTokenAsync(tokenModel.Token, refreshToken);

            if (newAccessToken == null || newRefreshToken == null)
            {
                return Unauthorized();
            }

            // Update the refresh token cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // Change to true in production (HTTPS)
                SameSite = SameSiteMode.Strict, // Required for cross-site cookies
                Expires = DateTime.UtcNow.AddDays(30)
            };
            Response.Cookies.Append("refreshToken", newRefreshToken, cookieOptions);

            return Ok(new { Token = newAccessToken });
        }

        [HttpPut("user_id={userId}")]
        [Authorize]
        public async Task<IActionResult> Update(int userId, [FromBody] UserUpdateDto? userUpdateDto)
        {
            try
            {
                if (userUpdateDto != null) await _userService.UpdateUserAsync(userId, userUpdateDto);
                return Ok(new { message = "User updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Update failed: {ex.Message}" });
            }
        }

        [HttpDelete("user_id={userId}")]
        [Authorize]
        public async Task<IActionResult> Delete(int userId)
        {
            try
            {
                await _userService.DeleteUserAsync(userId);
                return Ok(new { message = "User deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Delete failed: {ex.Message}" });
            }
        }
    }
}
